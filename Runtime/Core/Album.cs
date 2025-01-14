using System;
using UdonSharp;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Data;
using VRC.SDK3.StringLoading;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace UriAlbum.Runtime.Core
{
    [AddComponentMenu("UriAlbum/Album")]
    public class Album : UdonSharpBehaviour
    {
        // User provided options
        [SerializeField] private string _groupId;
        [SerializeField] private string _albumName;

        public string GroupId => _groupId;
        public string AlbumName => _albumName;

        // Compile time serialized values
        [SerializeField] private bool _isSet;
        [SerializeField] private VRCUrl _metadataUrl;
        [SerializeField] private VRCUrl[] _atlasUrls;
        [SerializeField] private VRCUrl _potatoUrl;

        public bool IsSet => _isSet;

        [SerializeField] private Prefabs _prefabs;
        public Prefabs Prefabs => _prefabs;

        // Runtime
        private Metadata.Album _metadata;

        private Atlas _potato;
        private Atlas[] _atlases;
        private Image[] _images;

        private readonly DataDictionary tagSubscriptions = new DataDictionary(); // Tag -> ImageSubscription
        private readonly DataList nonTagSubscriptions = new DataList();
        private readonly DataDictionary linkedSubscriptions = new DataDictionary(); // ID -> ImageSubscription

        private readonly DataList atlasLoadQueue = new DataList();

        public Subscription SubscribeTagImage(UdonSharpBehaviour target, string tag)
        {
            var subscription = Subscription.Create(this, target);
            tagSubscriptions[tag] = subscription;
            return subscription;
        }

        public Subscription SubscribeImage(UdonSharpBehaviour target)
        {
            var subscription = Subscription.Create(this, target);
            nonTagSubscriptions.Add(subscription);
            return subscription;
        }

        private void Start()
        {
            if (!IsSet) return;
            LoadMetadata();
        }

        private void LoadMetadata()
        {
            VRCStringDownloader.LoadUrl(_metadataUrl, (IUdonEventReceiver) this);
        }

        private void OnMetadataStringLoaded(string text)
        {
            var metadataObject = Instantiate(Prefabs.MetadataAlbum.gameObject, transform);
            _metadata = metadataObject.GetComponent<Metadata.Album>();
            if (VRCJson.TryDeserializeFromJson(text, out var data))
                _metadata.Apply(this, data);

            CreateAtlases();
            CreatePotato();
            LinkSubscriptions();
            InitializeAtlasLoadQueue();

            _potato.Load(_potatoUrl);
        }

        public override void OnStringLoadSuccess(IVRCStringDownload result)
        {
            if (result.Url.Equals(_metadataUrl)) OnMetadataStringLoaded(result.Result);
        }

        private void CreateAtlases()
        {
            _atlases = new Atlas[_metadata.Atlases.Length];
            for (var i = 0; i < _metadata.Atlases.Length; i++)
            {
                _atlases[i] = Atlas.Create(this, _metadata.Atlases[i]);
            }
        }

        private void CreatePotato()
        {
            var potatoMetadataObject = Instantiate(Prefabs.MetadataAtlas.gameObject, transform);
            var potatoMetadata = potatoMetadataObject.GetComponent<Metadata.Atlas>();

            var imageCount = 0;
            foreach (var atlas in _atlases) imageCount += atlas.Images.Length;

            var n = Mathf.CeilToInt(Mathf.Sqrt(_atlases.Length));
            var size = (float) _metadata.PotatoSize / n;
            var scale = 1f / n;

            potatoMetadata.Images = new Metadata.Image[imageCount];

            var imageIndex = 0;
            for (var atlasIndex = 0; atlasIndex < _atlases.Length; atlasIndex++)
            {
                var atlas = _atlases[atlasIndex];

                var sx = atlasIndex % n * size;
                var sy = (int) ((float) atlasIndex / n) * size;

                foreach (var image in atlas.Images)
                {
                    var potatoImageMetadataObject = Instantiate(Prefabs.MetadataImage.gameObject, transform);
                    var potatoImageMetadata = potatoImageMetadataObject.GetComponent<Metadata.Image>();
                    potatoImageMetadata.ID = image.Metadata.ID;
                    potatoImageMetadata.Tag = image.Metadata.Tag;
                    potatoImageMetadata.CreatedAt = image.Metadata.CreatedAt;
                    potatoImageMetadata.X = (int) (image.Metadata.X * scale + sx);
                    potatoImageMetadata.Y = (int) (image.Metadata.Y * scale + sy);
                    potatoImageMetadata.Width = (int) (image.Metadata.Width * scale);
                    potatoImageMetadata.Height = (int) (image.Metadata.Height * scale);
                    potatoMetadata.Images[imageIndex] = potatoImageMetadata;
                    imageIndex++;
                }
            }

            _potato = Atlas.Create(this, potatoMetadata);
        }

        private void LoadNextAtlas()
        {
            if (atlasLoadQueue.Count == 0) return;
            var index = atlasLoadQueue[0].Int;
            _atlases[index].Load(_atlasUrls[index]);
            atlasLoadQueue.RemoveAt(0);
        }

        public void PrioritizeAtlas(Atlas atlas)
        {
            // If already loaded, do nothing
            if (atlas.Loaded) return;

            // Find index of atlas in queue
            var index = Array.IndexOf(_atlases, atlas);
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
                    var subscription = (Subscription) subscriptionToken.Reference;
                    subscription.Notify(image);
                }
            }
        }

        private void LinkSubscriptions()
        {
            var images = new DataList();
            foreach (var atlas in _atlases)
            foreach (var image in atlas.Images)
                images.Add(image);

            // Link tagged images
            var tags = tagSubscriptions.GetKeys();
            for (var i = 0; i < tags.Count; i++)
            {
                var tag = tags[i];
                var subscription = (Subscription) tagSubscriptions[tag].Reference;

                for (var j = 0; j < images.Count; j++)
                {
                    var image = (Image) images[j].Reference;
                    if (image.Metadata.Tag == tag)
                    {
                        linkedSubscriptions[image.Metadata.ID] = subscription;
                        subscription.OriginalAtlas = image.Atlas;
                        break;
                    }
                }
            }

            // Link other images
            while (images.Count > 0 && nonTagSubscriptions.Count > 0)
            {
                var image = (Image) images[0].Reference;
                images.RemoveAt(0);
                if (IsImageSubscribed(image)) continue;

                var subscription = (Subscription) nonTagSubscriptions[0].Reference;
                nonTagSubscriptions.RemoveAt(0);
                linkedSubscriptions[image.Metadata.ID] = subscription;
                subscription.OriginalAtlas = image.Atlas;
            }
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
            for (var i = 0; i < _atlases.Length; i++)
            {
                var atlas = _atlases[i];
                if (IsAtlasSubscribed(atlas)) atlasLoadQueue.Add(i);
            }
        }

        public void ResetPrepared()
        {
            var children = new Transform[transform.childCount];
            for (var i = 0; i < transform.childCount; i++) children[i] = transform.GetChild(i);
            foreach (var child in children) DestroyImmediate(child.gameObject);
            _isSet = false;
        }

        private void Reset()
        {
            ResetPrepared();
        }
    }
}