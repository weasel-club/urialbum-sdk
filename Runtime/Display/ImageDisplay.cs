using System;
using UdonSharp;
using UnityEngine;
using UriAlbum.Runtime.Core;
using VRC.SDKBase;

namespace UriAlbum.Runtime.Display
{
    [AddComponentMenu("UriAlbum/Display/Image Display")]
    [RequireComponent(typeof(Renderer))]
    public class ImageDisplay : UdonSharpBehaviour
    {
        // User provided options
        public Album _album;
        public bool _useTag;
        public string _tag;

        // Editor defined options
        public int _order;

        private Subscription _subscription;
        private Renderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _renderer.material.color = Color.clear;
            if (_useTag) _subscription = _album.SubscribeImage(this, _tag);
            else _subscription = _album.SubscribeImage(this);
        }

        private void Apply(Image image)
        {
            var material = _renderer.material;
            var texture = image.Atlas.Texture;
            material.mainTexture = image.Atlas.Texture;

            // Display part of the texture (from left-top corner)
            material.mainTextureScale = new Vector2((float) image.Metadata.Width / texture.width,
                (float) image.Metadata.Height / texture.height);
            var offsetX = image.Metadata.X / (float) texture.width;
            var offsetY = (texture.height - image.Metadata.Y - image.Metadata.Height) / (float) texture.height;
            material.mainTextureOffset = new Vector2(offsetX, offsetY);
            _renderer.material.color = Color.white;
        }

        public void OnImageUpdate()
        {
            var image = _subscription.Image;
            Apply(image);
        }

        private void Update()
        {
            if (!_subscription.Linked) return;
            if (_subscription.OriginalAtlas.Loaded || _subscription.OriginalAtlas.Prioritized) return;
            var player = Networking.LocalPlayer;
            var distance = Vector3.Distance(player.GetPosition(), transform.position);
            if (distance >= 5f) return;
            _subscription.OriginalAtlas.Prioritize();
        }
    }
}