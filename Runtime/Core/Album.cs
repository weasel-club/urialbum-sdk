using System;
using UdonSharp;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;
using Random = UnityEngine.Random;

namespace URIAlbum.Runtime.Core
{
    [AddComponentMenu("")]
    public class Album : UdonSharpBehaviour
    {
        // Configured by editor
        public string groupId;
        public string groupName;
        public string albumId;
        public string albumName;
        public VRCUrl metadataUrl;
        public VRCUrl[] atlasUrls;
        public VRCUrl potatoUrl;
        public Prefabs prefabs;


        // Runtime
        private Metadata.Album metadata;
        private Atlas potato;
        private Atlas[] atlases;
        private Image[] images;


        private readonly DataDictionary tagSubscriptions = new DataDictionary(); // Tag -> DataList<ImageSubscription>
        private readonly DataList nonTagSubscriptions = new DataList();
        private readonly DataDictionary linkedSubscriptions = new DataDictionary(); // ID -> ImageSubscription

        private readonly DataList atlasLoadQueue = new DataList();

        private bool loadStarted;
        private float waitSeedTime;

        [UdonSynced] private int seed = -1;

        public Subscription SubscribeImage(UdonSharpBehaviour target, string tag = null)
        {
            var subscription = Subscription.Create(this, target);

            if (tag != null)
            {
                var list = tagSubscriptions.ContainsKey(tag) ? tagSubscriptions[tag].DataList : new DataList();
                list.Add(subscription);
                tagSubscriptions[tag] = list;
            }
            else
            {
                nonTagSubscriptions.Add(subscription);
            }

            return subscription;
        }

        private void Start()
        {
            if (Networking.IsMaster) seed = Random.Range(0, int.MaxValue);
        }

        private void Update()
        {
            if (loadStarted) return;

            // If seed is not synced yet, wait for it
            if (seed == -1) return;

            // Start load
            loadStarted = true;
            LoadMetadata();
        }

        private void LoadMetadata()
        {
            VRCStringDownloader.LoadUrl(metadataUrl, (IUdonEventReceiver)this);
        }

        private void OnMetadataStringLoaded(string text)
        {
            var metadataObject = Instantiate(prefabs.metadataAlbum.gameObject, transform);
            metadata = metadataObject.GetComponent<Metadata.Album>();
            if (VRCJson.TryDeserializeFromJson(text, out var data))
                metadata.Apply(this, data);

            CreateAtlases();
            CreatePotato();
            LinkSubscriptions();
            InitializeAtlasLoadQueue();

            potato.Load(potatoUrl);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            if (result.Url.Equals(metadataUrl)) OnMetadataStringLoaded(result.Result);
        }

        public override void OnStringLoadError(IVRCStringDownload result)
        {
            Debug.LogError($"[URIAlbum] Failed to load album metadata from {result.Url}: {result.Error}");
        }

        private void CreateAtlases()
        {
            atlases = new Atlas[metadata.Atlases.Length];
            for (var i = 0; i < metadata.Atlases.Length; i++)
            {
                atlases[i] = Atlas.Create(this, metadata.Atlases[i]);
            }
        }

        private void CreatePotato()
        {
            var potatoMetadataObject = Instantiate(prefabs.metadataAtlas.gameObject, transform);
            var potatoMetadata = potatoMetadataObject.GetComponent<Metadata.Atlas>();

            var imageCount = 0;
            foreach (var atlas in atlases) imageCount += atlas.Images.Length;

            var n = Mathf.CeilToInt(Mathf.Sqrt(atlases.Length));
            var size = (float)metadata.PotatoSize / n;
            var scale = 1f / n;

            potatoMetadata.Images = new Metadata.Image[imageCount];

            var imageIndex = 0;
            for (var atlasIndex = 0; atlasIndex < atlases.Length; atlasIndex++)
            {
                var atlas = atlases[atlasIndex];

                var sx = atlasIndex % n * size;
                var sy = (int)((float)atlasIndex / n) * size;

                foreach (var image in atlas.Images)
                {
                    var potatoImageMetadataObject = Instantiate(prefabs.metadataImage.gameObject, transform);
                    var potatoImageMetadata = potatoImageMetadataObject.GetComponent<Metadata.Image>();
                    potatoImageMetadata.ID = image.Metadata.ID;
                    potatoImageMetadata.Tag = image.Metadata.Tag;
                    potatoImageMetadata.CreatedAt = image.Metadata.CreatedAt;
                    potatoImageMetadata.X = (int)(image.Metadata.X * scale + sx);
                    potatoImageMetadata.Y = (int)(image.Metadata.Y * scale + sy);
                    potatoImageMetadata.Width = (int)(image.Metadata.Width * scale);
                    potatoImageMetadata.Height = (int)(image.Metadata.Height * scale);
                    potatoMetadata.Images[imageIndex] = potatoImageMetadata;
                    imageIndex++;
                }
            }

            potato = Atlas.Create(this, potatoMetadata);
        }

        private void LoadNextAtlas()
        {
            if (atlasLoadQueue.Count == 0) return;
            var index = atlasLoadQueue[0].Int;
            atlases[index].Load(atlasUrls[index]);
            atlasLoadQueue.RemoveAt(0);
        }

        public void PrioritizeAtlas(Atlas atlas)
        {
            // If already loaded, do nothing
            if (atlas.Loaded) return;

            // Find index of atlas in queue
            var index = Array.IndexOf(atlases, atlas);
            if (index == -1) return;

            // If already first in queue, do nothing
            if (atlasLoadQueue.Count > 0 && atlasLoadQueue[0].Int == index) return;

            // Move to front of queue
            if (atlasLoadQueue.Remove(index))
            {
                atlasLoadQueue.Insert(0, index);
            }
        }

        public void OnAtlasLoaded(Atlas atlas)
        {
            LoadNextAtlas();

            // Notify subscriptions
            foreach (var image in atlas.Images)
            {
                if (linkedSubscriptions.TryGetValue(image.Metadata.ID, out var subscriptionToken))
                {
                    var subscription = (Subscription)subscriptionToken.Reference;
                    subscription.Notify(image);
                }
            }
        }

        private static int FindImageWithTag(DataList images, string tag)
        {
            for (var i = 0; i < images.Count; i++)
            {
                var image = (Image)images[i].Reference;
                if (image.Metadata.Tag == tag) return i;
            }

            return -1;
        }

        private void ShuffleDataList(DataList list)
        {
            // Seeding for instance deterministic shuffle
            Random.InitState(seed);

            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }

        private bool IsTagged(Image image)
        {
            return image.Metadata.Tag != null && image.Metadata.Tag.Length > 0;
        }

        private void LinkSubscriptions()
        {
            // 태그된 이미지와 태그되지 않은 이미지를 분리
            var taggedImages = new DataList();
            var untaggedImages = new DataList();

            foreach (var atlas in atlases)
                foreach (var image in atlas.Images)
                    if (IsTagged(image)) taggedImages.Add(image);
                    else untaggedImages.Add(image);

            // 각각 셔플
            ShuffleDataList(taggedImages);
            ShuffleDataList(untaggedImages);

            // 태그된 이미지 연결
            var tags = tagSubscriptions.GetKeys();
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i].String;
                var subscriptions = tagSubscriptions[tag].DataList;

                for (var j = 0; j < subscriptions.Count; j++)
                {
                    var subscription = (Subscription)subscriptions[j].Reference;

                    // 해당 태그를 가진 이미지 찾기
                    var imageIndex = FindImageWithTag(taggedImages, tag);
                    if (imageIndex == -1) break;

                    var image = (Image)taggedImages[imageIndex].Reference;
                    taggedImages.RemoveAt(imageIndex);
                    LinkImageSubscription(subscription, image);
                }
            }

            // 태그되지 않은 이미지 연결
            while (untaggedImages.Count > 0 && nonTagSubscriptions.Count > 0)
            {
                var image = (Image)untaggedImages[0].Reference;
                var subscription = (Subscription)nonTagSubscriptions[0].Reference;

                LinkImageSubscription(subscription, image);
                untaggedImages.RemoveAt(0);
                nonTagSubscriptions.RemoveAt(0);
            }
        }

        private void LinkImageSubscription(Subscription subscription, Image image)
        {
            linkedSubscriptions[image.Metadata.ID] = subscription;
            subscription.OriginalAtlas = image.Atlas;
        }

        private bool IsImageSubscribed(Image image)
        {
            return linkedSubscriptions.ContainsKey(image.Metadata.ID);
        }

        private bool IsAtlasSubscribed(Atlas atlas)
        {
            foreach (var image in atlas.Images)
                if (IsImageSubscribed(image))
                    return true;

            return false;
        }

        private void InitializeAtlasLoadQueue()
        {
            for (var i = 0; i < atlases.Length; i++)
            {
                var atlas = atlases[i];
                if (IsAtlasSubscribed(atlas)) atlasLoadQueue.Add(i);
            }
        }

    }
}