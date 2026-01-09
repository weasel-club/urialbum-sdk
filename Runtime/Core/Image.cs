using UdonSharp;
using UnityEngine;

namespace URIAlbum.Runtime.Core
{
    [AddComponentMenu("")]
    public class Image : UdonSharpBehaviour
    {
        private Metadata.Image _metadata;
        private Album _album;
        private Atlas _atlas;

        public Metadata.Image Metadata => _metadata;
        public Atlas Atlas => _atlas;

        public static Image Create(Album album, Atlas atlas, Metadata.Image metadata)
        {
            var imageObject = Instantiate(album.prefabs.image.gameObject, album.transform);
            var image = imageObject.GetComponent<Image>();
            image._album = album;
            image._metadata = metadata;
            image._atlas = atlas;
            return image;
        }
    }
}
