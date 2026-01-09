using System;
using UdonSharp;
using UnityEngine;
using URIAlbum.Runtime.Core;
using VRC.SDKBase;

namespace URIAlbum.Runtime.Display
{
    [AddComponentMenu("URIAlbum/Image Display")]
    [RequireComponent(typeof(Renderer))]
    public class ImageDisplay : UdonSharpBehaviour
    {
        // User provided options
        public AlbumSetting setting;
        public string albumId;
        new public string tag;

        // Editor defined options
        public int order;

        private Subscription subscription;
        new private Renderer renderer;

        private Album GetAlbum()
        {
            for (var i = 0; i < setting.albums.Length; i++)
            {
                var album = setting.albums[i];
                if (album.albumId == albumId) return album;
            }

            return null;
        }

        private void Start()
        {
            var album = GetAlbum();
            renderer = GetComponent<Renderer>();
            renderer.material.color = Color.clear;
            if (tag != null && tag.Trim() != string.Empty) subscription = album.SubscribeImage(this, tag);
            else subscription = album.SubscribeImage(this);
        }

        private void Apply(Image image)
        {
            var material = renderer.material;
            var texture = image.Atlas.Texture;
            material.mainTexture = image.Atlas.Texture;

            // Display part of the texture (from left-top corner)
            material.mainTextureScale = new Vector2((float)image.Metadata.Width / texture.width,
                (float)image.Metadata.Height / texture.height);
            var offsetX = image.Metadata.X / (float)texture.width;
            var offsetY = (texture.height - image.Metadata.Y - image.Metadata.Height) / (float)texture.height;
            material.mainTextureOffset = new Vector2(offsetX, offsetY);
            renderer.material.color = Color.white;
        }

        public void OnImageUpdate()
        {
            var image = subscription.Image;
            Apply(image);
        }

        private void Update()
        {
            if (!subscription.Linked) return;
            if (subscription.OriginalAtlas.Loaded || subscription.OriginalAtlas.Prioritized) return;
            var player = Networking.LocalPlayer;
            var distance = Vector3.Distance(player.GetPosition(), transform.position);
            if (distance >= 5f) return;
            subscription.OriginalAtlas.Prioritize();
        }
    }
}
