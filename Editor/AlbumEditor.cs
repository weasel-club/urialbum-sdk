#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UriAlbum.Runtime.Core;

namespace UriAlbum.Editor
{
    [CustomEditor(typeof(Album))]
    public class AlbumEditor : UnityEditor.Editor
    {
        private const int PreparedAtlasUrls = 512;
        private const string APIUrl = "https://api.urialbum.com";
        private const string APIVersion = "v0";

        private static GameObject CreateObject(string objectName, GameObject parent)
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(parent.transform);
            return instance;
        }

        private static T CreateObject<T>(string objectName, GameObject parent) where T : Component
        {
            return CreateObject(objectName, parent).AddComponent<T>();
        }

        private static void SetupPrefabs(Prefabs prefabs, SerializedObject serializedObject)
        {
            var metadataAlbumProperty = serializedObject.FindProperty("_metadataAlbum");
            var metadataAtlasProperty = serializedObject.FindProperty("_metadataAtlas");
            var metadataImageProperty = serializedObject.FindProperty("_metadataImage");
            var subscriptionProperty = serializedObject.FindProperty("_subscription");
            var atlasProperty = serializedObject.FindProperty("_atlas");
            var imageProperty = serializedObject.FindProperty("_image");

            var metadataAlbum = CreateObject<Runtime.Core.Metadata.Album>("MetadataAlbum", prefabs.gameObject);
            var metadataAtlas = CreateObject<Runtime.Core.Metadata.Atlas>("MetadataAtlas", prefabs.gameObject);
            var metadataImage = CreateObject<Runtime.Core.Metadata.Image>("MetadataImage", prefabs.gameObject);
            var imageSubscription = CreateObject<Subscription>("Subscription", prefabs.gameObject);
            var atlas = CreateObject<Atlas>("Atlas", prefabs.gameObject);
            var image = CreateObject<Image>("Image", prefabs.gameObject);

            metadataAlbumProperty.objectReferenceValue = metadataAlbum;
            metadataAtlasProperty.objectReferenceValue = metadataAtlas;
            metadataImageProperty.objectReferenceValue = metadataImage;
            subscriptionProperty.objectReferenceValue = imageSubscription;
            atlasProperty.objectReferenceValue = atlas;
            imageProperty.objectReferenceValue = image;

            EditorUtility.SetDirty(prefabs);
            serializedObject.ApplyModifiedProperties();
        }

        private static void Setup(Album album, SerializedObject serializedObject)
        {
            album.ResetPrepared();

            var groupIdProperty = serializedObject.FindProperty("_groupId");
            var albumNameProperty = serializedObject.FindProperty("_albumName");
            var metadataUrlProperty = serializedObject.FindProperty("_metadataUrl");
            var potatoUrlProperty = serializedObject.FindProperty("_potatoUrl");
            var atlasUrlsProperty = serializedObject.FindProperty("_atlasUrls");
            var prefabsProperty = serializedObject.FindProperty("_prefabs");
            var isSetProperty = serializedObject.FindProperty("_isSet");

            groupIdProperty.stringValue = groupIdProperty.stringValue.Trim();
            albumNameProperty.stringValue = albumNameProperty.stringValue.Trim();

            var albumBaseUrl = $"{APIUrl}/{APIVersion}/groups/{album.GroupId}/albums/{album.AlbumName}";

            metadataUrlProperty.FindPropertyRelative("url").stringValue = albumBaseUrl;
            potatoUrlProperty.FindPropertyRelative("url").stringValue = $"{albumBaseUrl}/potato";
            atlasUrlsProperty.arraySize = PreparedAtlasUrls;
            for (var i = 0; i < PreparedAtlasUrls; i++)
            {
                atlasUrlsProperty.GetArrayElementAtIndex(i).FindPropertyRelative("url").stringValue =
                    $"{albumBaseUrl}/{i}";
            }

            var prefabs = CreateObject<Prefabs>("Prefabs", album.gameObject);
            prefabsProperty.objectReferenceValue = prefabs;
            SetupPrefabs(prefabs, new SerializedObject(prefabsProperty.objectReferenceValue));

            isSetProperty.boolValue = true;

            EditorUtility.SetDirty(album);
            serializedObject.ApplyModifiedProperties();
        }


        public override void OnInspectorGUI()
        {
            EditorUtil.DrawHeader();

            var album = (Album) target;

            if (album.IsSet) EditorGUI.BeginDisabledGroup(true);

            var groupIdProperty = serializedObject.FindProperty("_groupId");
            var albumNameProperty = serializedObject.FindProperty("_albumName");

            EditorGUILayout.PropertyField(groupIdProperty, new GUIContent("Group ID"));
            EditorGUILayout.PropertyField(albumNameProperty, new GUIContent("Album Name"));

            EditorGUILayout.Space();

            if (album.IsSet) EditorGUI.EndDisabledGroup();

            if (album.IsSet)
            {
                if (GUILayout.Button("Reset")) album.ResetPrepared();
            }
            else if (GUILayout.Button("Setup")) Setup(album, serializedObject);

            serializedObject.ApplyModifiedProperties();
            EditorUtil.DrawFooter();
        }
    }
}

#endif