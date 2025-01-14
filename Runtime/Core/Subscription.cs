using System;
using UdonSharp;
using UnityEngine;

namespace UriAlbum.Runtime.Core
{
    [AddComponentMenu("")]
    public class Subscription : UdonSharpBehaviour
    {
        private UdonSharpBehaviour _target;
        private Album _album;
        private Image _image;

        public Image Image => _image;
        public bool Linked => _image != null;

        [NonSerialized] public Atlas OriginalAtlas;

        public static Subscription Create(Album album, UdonSharpBehaviour target)
        {
            var subscriptionObject = Instantiate(album.Prefabs.Subscription.gameObject, album.transform);
            subscriptionObject.name = "Subscription";
            var subscription = subscriptionObject.GetComponent<Subscription>();
            subscription._album = album;
            subscription._target = target;
            return subscription;
        }

        public void Notify(Image image)
        {
            _image = image;
            _target.SendCustomEvent("OnImageUpdate");
        }
    }
}