using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon.Common.Interfaces;

namespace UriAlbum.Runtime.Core
{
    [AddComponentMenu("")]
    public class Atlas : UdonSharpBehaviour
    {
        [NonSerialized] public Texture2D Texture;
        private VRCImageDownloader _downloader;

        private Metadata.Atlas _metadata;
        private Album _album;
        private Image[] _images;

        public Image[] Images => _images;
        public Metadata.Atlas Metadata => _metadata;

        private bool _prioritized;
        public bool Prioritized => _prioritized;

        public bool Loaded => Texture != null;

        public static Atlas Create(Album album, Metadata.Atlas metadata)
        {
            var atlasObject = Instantiate(album.Prefabs.Atlas.gameObject, album.transform);
            var atlas = atlasObject.GetComponent<Atlas>();
            atlas._metadata = metadata;
            atlas._album = album;
            atlas._images = new Image[metadata.Images.Length];
            for (var i = 0; i < metadata.Images.Length; i++)
            {
                var image = Image.Create(album, atlas, metadata.Images[i]);
                atlas._images[i] = image;
            }

            return atlas;
        }

        public override void OnImageLoadSuccess(IVRCImageDownload result)
        {
            Texture = result.Result;
            _album.OnAtlasLoaded(this);
        }

        public void Load(VRCUrl url)
        {
            _downloader = new VRCImageDownloader();
            _downloader.DownloadImage(url, null, (IUdonEventReceiver) this);
        }

        public void Prioritize()
        {
            if (_prioritized) return;
            _prioritized = true;
            _album.PrioritizeAtlas(this);
            Debug.Log($"Prioritized atlas {this}");
        }
    }
}