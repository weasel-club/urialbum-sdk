#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Networking;
using URIAlbum.Runtime.Core;
using VRC.SDKBase;

namespace URIAlbum.Editor
{
    [Serializable]
    public class AlbumInfo
    {
        public string id;
        public string name;
    }

    [Serializable]
    public class GroupInfo
    {
        public string id;
        public string name;
        public List<AlbumInfo> albums;
    }

    [Serializable]
    public class GroupInfoResponse
    {
        public GroupInfo group;
    }

    [CustomEditor(typeof(AlbumSetting))]
    public class AlbumSettingEditor : UnityEditor.Editor
    {
        private bool loading = false;
        private static GUIStyle groupHeaderStyle;
        private static GUIStyle albumListStyle;

        private class AlbumEntry
        {
            public string Id;
            public string Name;
            public string GroupId;
            public int ImageCount;
        }

        private static void EnsureStyles()
        {
            if (groupHeaderStyle != null && albumListStyle != null) return;

            groupHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 12
            };

            albumListStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 6, 6)
            };
        }


        private static Prefabs CreatePrefabs(AlbumSetting setting)
        {
            var prefabs = EditorUtil.CreateObject<Prefabs>("Prefabs", setting.gameObject);
            prefabs.metadataAlbum = EditorUtil.CreateObject<URIAlbum.Runtime.Core.Metadata.Album>("MetadataAlbum", prefabs.gameObject);
            prefabs.metadataAtlas = EditorUtil.CreateObject<URIAlbum.Runtime.Core.Metadata.Atlas>("MetadataAtlas", prefabs.gameObject);
            prefabs.metadataImage = EditorUtil.CreateObject<URIAlbum.Runtime.Core.Metadata.Image>("MetadataImage", prefabs.gameObject);
            prefabs.subscription = EditorUtil.CreateObject<Subscription>("Subscription", prefabs.gameObject);
            prefabs.atlas = EditorUtil.CreateObject<Atlas>("Atlas", prefabs.gameObject);
            prefabs.image = EditorUtil.CreateObject<Image>("Image", prefabs.gameObject);
            EditorUtility.SetDirty(prefabs);
            return prefabs;
        }

        private static async Task<bool> ConfigureAsync(AlbumSetting setting)
        {
            if (setting.key == null || setting.key.Trim() == string.Empty)
            {
                return false;
            }

            var url = $"{EditorUtil.APIUrl}/groups?key={UnityWebRequest.EscapeURL(setting.key)}";
            using var client = new HttpClient();
            var response = await client.GetStringAsync(url);

            var data = JsonUtility.FromJson<GroupInfoResponse>(response);
            var groupInfo = data.group;

            setting.Reset();
            setting.configured = true;

            var prefabs = CreatePrefabs(setting);

            var albums = new Album[groupInfo.albums.Count];

            for (var i = 0; i < groupInfo.albums.Count; i++)
            {
                var albumInfo = groupInfo.albums[i];
                var album = EditorUtil.CreateObject<Album>($"Album ({albumInfo.name})", setting.gameObject);
                album.groupId = groupInfo.id;
                album.albumId = albumInfo.id;
                album.groupName = groupInfo.name;
                album.albumName = albumInfo.name;
                album.metadataUrl = new VRCUrl($"{EditorUtil.APIUrl}/groups/{groupInfo.id}/albums/{albumInfo.id}/atlases?key={setting.key}");
                album.atlasUrls = new VRCUrl[EditorUtil.PreparedAtlasUrls];
                for (var j = 0; j < album.atlasUrls.Length; j++)
                {
                    album.atlasUrls[j] = new VRCUrl($"{EditorUtil.APIUrl}/groups/{groupInfo.id}/albums/{albumInfo.id}/atlases/{j}?key={setting.key}");
                }
                album.potatoUrl = new VRCUrl($"{EditorUtil.APIUrl}/groups/{groupInfo.id}/albums/{albumInfo.id}/potato?key={setting.key}");
                album.prefabs = prefabs;
                albums[i] = album;
                EditorUtility.SetDirty(album);
            }
            setting.albums = albums;
            EditorUtility.SetDirty(setting);
            EditorSceneManager.MarkSceneDirty(setting.gameObject.scene);
            return true;
        }

        private async void Configure()
        {
            var setting = (AlbumSetting)target;
            if (setting.key == null || setting.key.Trim() == string.Empty)
            {
                return;
            }

            loading = true;
            try
            {
                await ConfigureAsync(setting);
            }
            finally
            {
                loading = false;
            }
        }

        public static async Task Refresh(AlbumSetting setting)
        {
            await ConfigureAsync(setting);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorUtil.DrawHeader();
            EnsureStyles();

            var setting = (AlbumSetting)target;

            EditorGUI.BeginDisabledGroup(loading);
            EditorGUILayout.BeginVertical(EditorUtil.BoxStyle);
            EditorGUILayout.LabelField(EditorUtil.Tr("AlbumSetting"), EditorUtil.HeaderStyle);
            EditorGUILayout.Space(4);
            if (setting.configured)
            {
                if (setting.albums.Length == 0)
                {
                    EditorGUILayout.HelpBox(EditorUtil.Tr("NoAlbumInGroup"), MessageType.Info);
                }
                else
                {
                    var groupName = setting.albums.First().groupName;

                    EditorGUILayout.LabelField($"{EditorUtil.Tr("Group")}: {groupName}", groupHeaderStyle);
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(EditorUtil.Tr("Albums"), EditorStyles.miniBoldLabel);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(12);
                    EditorGUILayout.BeginVertical(albumListStyle);
                    foreach (var album in setting.albums.OrderBy(a => a.albumName))
                    {
                        EditorGUILayout.LabelField($"ㄴ {album.albumName}");
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();

                }

                EditorGUILayout.Space(6);
                EditorGUILayout.BeginHorizontal();
                var resetContent = new GUIContent(EditorUtil.Tr("Reset"), EditorGUIUtility.IconContent("TreeEditor.Trash").image);
                var refreshContent = new GUIContent(EditorUtil.Tr("Refresh"), EditorGUIUtility.IconContent("Refresh").image);
                if (GUILayout.Button(resetContent, GUILayout.Height(24)))
                {
                    setting.Reset();
                    EditorUtility.SetDirty(setting);
                }
                if (GUILayout.Button(refreshContent, GUILayout.Height(24)))
                {
                    Configure();
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                setting.key = EditorGUILayout.TextField(EditorUtil.Tr("LinkKey"), setting.key);
                EditorGUILayout.Space(4);
                var configureContent = new GUIContent(EditorUtil.Tr("Configure"), EditorGUIUtility.IconContent("d_Toolbar Plus").image);
                if (GUILayout.Button(configureContent, GUILayout.Height(24)))
                {
                    Configure();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
            EditorUtil.DrawFooter();
        }
    }
}

#endif
