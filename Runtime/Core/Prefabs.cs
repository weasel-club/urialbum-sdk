using UdonSharp;
using UnityEngine;

namespace URIAlbum.Runtime.Core
{
    [AddComponentMenu("")]
    public class Prefabs : UdonSharpBehaviour
    {
        public Metadata.Album metadataAlbum;
        public Metadata.Atlas metadataAtlas;
        public Metadata.Image metadataImage;
        public Subscription subscription;
        public Atlas atlas;
        public Image image;
    }
}