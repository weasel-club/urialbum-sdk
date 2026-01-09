#if UNITY_EDITOR

using UnityEditor;

namespace URIAlbum.Editor
{
    public class HideInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is managed by the URIAlbum setting. Do not modify.",
                MessageType.Info);
        }
    }

    [CustomEditor(typeof(Runtime.Core.Album))]
    public class AlbumEditor : HideInspector { }


    [CustomEditor(typeof(Runtime.Core.Atlas))]
    public class CoreAtlasEditor : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Image))]
    public class CoreImageEditor : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Subscription))]
    public class CoreSubscriptionEditor : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Prefabs))]
    public class CorePrefabsEditor : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Metadata.Atlas))]
    public class MetadataAtlasEditor : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Metadata.Image))]
    public class MetadataImageEditor : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Metadata.Album))]
    public class MetadataAlbumEditor : HideInspector
    {
    }

}

#endif