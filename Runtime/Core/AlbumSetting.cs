using UdonSharp;
using UnityEngine;

namespace URIAlbum.Runtime.Core
{
    [AddComponentMenu("URIAlbum/Album Setting")]
    public class AlbumSetting : UdonSharpBehaviour
    {
        public string key;
        public bool configured;
        public Album[] albums;

        public void Reset()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
            configured = false;
        }
    }
}
