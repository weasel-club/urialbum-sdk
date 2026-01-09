using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;

namespace URIAlbum.Runtime.Core.Metadata
{
    [AddComponentMenu("")]
    public class Image : UdonSharpBehaviour
    {
        [NonSerialized] public string ID;
        [NonSerialized] public string Tag;
        [NonSerialized] public string CreatedAt;
        [NonSerialized] public int X;
        [NonSerialized] public int Y;
        [NonSerialized] public int Width;
        [NonSerialized] public int Height;
        [NonSerialized] public bool Rotated;

        public void Apply(Core.Album album, DataDictionary data)
        {
            ID = data["id"].String;
            CreatedAt = data["createdAt"].String;
            Tag = data["tag"].IsNull
                ? null
                : data["tag"].String;
            X = (int)data["x"].Double;
            Y = (int)data["y"].Double;
            Width = (int)data["width"].Double;
            Height = (int)data["height"].Double;
            Rotated = data["rotated"].Boolean;
        }
    }
}
