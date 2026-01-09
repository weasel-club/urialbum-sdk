#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEngine;
using URIAlbum.Runtime.Core;
using URIAlbum.Runtime.Display;

namespace URIAlbum.Editor
{
    [CustomEditor(typeof(ImageDisplay))]
    public class ImageDisplayEditor : UnityEditor.Editor
    {
        private bool loading = false;

        public override void OnInspectorGUI()
        {
            EditorUtil.DrawHeader();

            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.HelpBox(EditorUtil.Tr("PlayModeDisabled"), MessageType.Info);
                EditorUtil.DrawFooter();
                return;
            }

            DrawAlbumSelection();

            serializedObject.ApplyModifiedProperties();

            EditorUtil.DrawFooter();
        }

        private async void RefreshAlbumSetting()
        {
            var imageDisplay = (ImageDisplay)target;
            loading = true;
            try
            {
                await AlbumSettingEditor.Refresh(imageDisplay.setting);
            }
            finally
            {
                loading = false;
            }
        }

        private void DrawAlbumSelection()
        {
            var imageDisplay = (ImageDisplay)target;

            EditorGUI.BeginDisabledGroup(loading);
            var settings = FindObjectsOfType<AlbumSetting>();
            var albums = FindObjectsOfType<Album>();
            EditorGUILayout.BeginVertical(EditorUtil.BoxStyle);
            EditorGUILayout.LabelField(EditorUtil.Tr("ImageDisplaySetting"), EditorUtil.HeaderStyle);
            EditorGUILayout.Space(8);
            if (albums.Length == 0)
            {
                EditorGUILayout.HelpBox(EditorUtil.Tr("NoAlbumFound"), MessageType.Warning);
            }
            else
            {
                var settingByAlbum = albums.ToDictionary(a => a, a =>
                {
                    foreach (var setting in settings)
                    {
                        if (setting.albums.Contains(a)) return setting;
                    }

                    return null;
                });

                var options = albums.Select(a => $"{a.groupName} / {a.albumName}").ToArray();
                var currentIndex = albums.ToList().FindIndex(e => e.albumId == imageDisplay.albumId);
                var selectedIndex = EditorGUILayout.Popup(EditorUtil.Tr("Album"), currentIndex, options);
                if (selectedIndex >= 0 && selectedIndex < albums.Length)
                {
                    var album = albums[selectedIndex];
                    var nextSetting = settingByAlbum[album];
                    if (imageDisplay.setting != nextSetting || imageDisplay.albumId != album.albumId)
                    {
                        imageDisplay.setting = nextSetting;
                        imageDisplay.albumId = album.albumId;
                        EditorUtility.SetDirty(imageDisplay);
                    }
                }
            }
            DrawTagSelection();
            EditorGUILayout.Space(6);
            using (new EditorGUI.DisabledScope(imageDisplay.setting == null))
            {
                var refreshContent = new GUIContent(EditorUtil.Tr("RefreshAlbumSetting"), EditorGUIUtility.IconContent("Refresh").image);
                if (GUILayout.Button(refreshContent, GUILayout.Height(24)))
                {
                    RefreshAlbumSetting();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }

        private void DrawTagSelection()
        {
            var imageDisplay = (ImageDisplay)target;
            var displays = FindObjectsOfType<ImageDisplay>();
            var tagOptions = displays
                .Select(display => display.tag)
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct()
                .OrderBy(tag => tag)
                .ToList();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(EditorUtil.Tr("Tag"), EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(EditorUtil.Tr("Tag"), GUILayout.Width(80));
            var nextTag = EditorGUILayout.TextField(imageDisplay.tag);
            EditorGUILayout.EndHorizontal();
            if (nextTag != imageDisplay.tag)
            {
                imageDisplay.tag = nextTag;
                EditorUtility.SetDirty(imageDisplay);
            }

            if (tagOptions.Count == 0)
            {
                return;
            }

            var options = new[] { EditorUtil.Tr("Custom"), EditorUtil.Tr("None") }.Concat(tagOptions).ToArray();
            var currentIndex = 0;
            if (string.IsNullOrWhiteSpace(imageDisplay.tag))
            {
                currentIndex = 1;
            }
            else
            {
                var tagIndex = tagOptions.IndexOf(imageDisplay.tag);
                if (tagIndex >= 0) currentIndex = tagIndex + 2;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(EditorUtil.Tr("TagPreset"), GUILayout.Width(80));
            var selectedIndex = EditorGUILayout.Popup(currentIndex, options);
            EditorGUILayout.EndHorizontal();

            if (selectedIndex != currentIndex)
            {
                if (selectedIndex == 1)
                {
                    imageDisplay.tag = string.Empty;
                }
                else if (selectedIndex >= 2)
                {
                    imageDisplay.tag = tagOptions[selectedIndex - 2];
                }
                EditorUtility.SetDirty(imageDisplay);
            }
        }
    }
}

#endif
