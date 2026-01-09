using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace URIAlbum.Runtime.Core.Metadata
{
    [AddComponentMenu("")]
    public class Album : UdonSharpBehaviour
    {
        [NonSerialized] public int PotatoSize;
        [NonSerialized] public Atlas[] Atlases;

        public void Apply(Core.Album album, DataToken data)
        {
            var dictionary = data.DataDictionary;

            PotatoSize = (int)dictionary["potatoSize"].Double;
            var atlases = dictionary["atlases"].DataList;
            Atlases = new Atlas[atlases.Count];
            for (var i = 0; i < atlases.Count; i++)
            {
                var atlasToken = atlases[i].DataDictionary;
                var atlasObject = Instantiate(album.prefabs.metadataAtlas.gameObject, album.transform);
                var atlas = atlasObject.GetComponent<Atlas>();
                atlas.Apply(album, atlasToken);
                Atlases[i] = atlas;
            }
        }
    }
}
