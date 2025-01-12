#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UriAlbum.Runtime.Core;

namespace UriAlbum.Editor
{
    public static class EditorUtil
    {
        private const string DashboardURL = "https://urialbum.com";
        private static List<Album> AlbumsInScene { get; set; }

        public static void UpdateAlbumsInScene()
        {
            if (EditorApplication.isPlaying)
            {
                AlbumsInScene = new List<Album>();
                return;
            }

            AlbumsInScene = new List<Album>(Object.FindObjectsOfType<Album>());
        }

        public static Album AlbumSelector(string label, Album currentAlbum)
        {
            if (AlbumsInScene == null) UpdateAlbumsInScene();
            var albums = AlbumsInScene;
            if (albums == null) return null;
            var albumNames = new string[albums.Count];
            for (var i = 0; i < albums.Count; i++)
            {
                albumNames[i] = albums[i].name;
            }

            var currentIndex = albums.IndexOf(currentAlbum);
            if (currentIndex == -1) currentIndex = 0;
            var newIndex = EditorGUILayout.Popup(label, currentIndex, albumNames);
            return albums[newIndex];
        }

        public static void DrawHeader()
        {
            EditorGUILayout.LabelField("UriAlbum", new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                normal = new GUIStyleState {textColor = Color.white}
            });
            EditorGUILayout.Space();
        }

        public static void DrawFooter()
        {
            EditorGUILayout.Space();
            if (GUILayout.Button("Open Dashboard")) Application.OpenURL(DashboardURL);
        }
    }
}

#endif