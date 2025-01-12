using UdonSharp;
using UnityEngine;
using UnityEngine.Serialization;

namespace UriAlbum.Runtime.Core
{
    [AddComponentMenu("")]
    public class Prefabs : UdonSharpBehaviour
    {
        [SerializeField] private Metadata.Album _metadataAlbum;
        [SerializeField] private Metadata.Atlas _metadataAtlas;
        [SerializeField] private Metadata.Image _metadataImage;
        [FormerlySerializedAs("_imageSubscription")] [SerializeField] private Subscription _subscription;
        [SerializeField] private Atlas _atlas;
        [SerializeField] private Image _image;

        public Metadata.Album MetadataAlbum => _metadataAlbum;
        public Metadata.Atlas MetadataAtlas => _metadataAtlas;
        public Metadata.Image MetadataImage => _metadataImage;
        public Subscription Subscription => _subscription;
        public Atlas Atlas => _atlas;
        public Image Image => _image;
    }
}