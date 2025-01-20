#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UriAlbum.Runtime.Core;
using UriAlbum.Runtime.Display;

namespace UriAlbum.Editor
{
    public static class EditorUtil
    {
        private const string DashboardURL = "https://urialbum.com";
        private static List<Album> AlbumsInScene { get; set; }

        static EditorUtil()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private static void OnHierarchyChanged()
        {
            UpdateAlbumsInScene();
            UpdateImageDisplayOrders();
        }

        public static void UpdateAlbumsInScene()
        {
            if (EditorApplication.isPlaying)
            {
                AlbumsInScene = new List<Album>();
                return;
            }

            AlbumsInScene = new List<Album>(Object.FindObjectsOfType<Album>());
        }

        private static T[] SortByHierarchyOrder<T>(T[] objects) where T: Component
        {
            return objects.OrderBy(GetHierarchyPath).ToArray();
        }

        private static string GetHierarchyPath<T >(T obj) where T: Component
        {
            var path = new List<string>();
            var current = obj.transform;
            
            while (current != null)
            {
                // GetSiblingIndex를 사용해 같은 레벨에서의 순서 유지
                path.Insert(0, current.GetSiblingIndex().ToString("D10"));
                current = current.parent;
            }
            
            return string.Join("/", path);
        }

        private static void UpdateImageDisplayOrders() {
            var imageDisplays = Object.FindObjectsOfType<ImageDisplay>();
            var sortedDisplays = SortByHierarchyOrder(imageDisplays);
            for (var i = 0; i < sortedDisplays.Length; i++)
            {
                var display = sortedDisplays[i];
                display._order = i;
            }
        }

        public static Album AlbumSelector(string label, Album currentAlbum)
        {
            if (AlbumsInScene == null) UpdateAlbumsInScene();
            var albums = AlbumsInScene;
            if (albums == null) return null;

            if (albums.Count == 0)
            {
                EditorGUILayout.HelpBox("No albums found in scene.", MessageType.Warning);
                return null;
            }

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