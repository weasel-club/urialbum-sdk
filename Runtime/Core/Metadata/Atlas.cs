using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace UriAlbum.Runtime.Core.Metadata
{
    [AddComponentMenu("")]
    public class Atlas : UdonSharpBehaviour
    {
        [NonSerialized] public int Size;
        [NonSerialized] public Image[] Images;

        public void Apply(Core.Album album, DataDictionary data)
        {
            var size = (int) data["size"].Double;
            var images = data["images"].DataList;
            Size = size;
            Images = new Image[images.Count];

            for (var j = 0; j < images.Count; j++)
            {
                var imageToken = images[j].DataDictionary;
                var imageObject = Instantiate(album.Prefabs.MetadataImage.gameObject, album.transform);
                var image = imageObject.GetComponent<Image>();
                image.Apply(album, imageToken);
                Images[j] = image;
            }
        }
    }
}