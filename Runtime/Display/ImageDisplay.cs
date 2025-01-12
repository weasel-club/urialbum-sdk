using UdonSharp;
using UnityEngine;
using UriAlbum.Runtime.Core;

namespace UriAlbum.Runtime.Display
{
    [AddComponentMenu("UriAlbum/Display/Image Display")]
    [RequireComponent(typeof(Renderer))]
    public class ImageDisplay : UdonSharpBehaviour
    {
        public Album _album;
        public bool _useTag;
        public string _tag;

        private Subscription _subscription;
        private Renderer _renderer;

        private void Start()
        {
            _renderer = GetComponent<Renderer>();
            _renderer.material.color = Color.clear;

            if (_useTag) _subscription = _album.SubscribeTagImage(this, _tag);
            else _subscription = _album.SubscribeImage(this);
        }

        public void Apply(Image image)
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
    }
}