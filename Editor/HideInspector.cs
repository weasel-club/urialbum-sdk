#if UNITY_EDITOR

using UnityEditor;
using UriAlbum.Runtime.Core.Metadata;

namespace UriAlbum.Editor
{
    public class HideInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This component is managed by the UriAlbum system. Do not modify.",
                MessageType.Info);
        }
    }

    [CustomEditor(typeof(Atlas))]
    public class MetadataAtlas : HideInspector
    {
    }

    [CustomEditor(typeof(Image))]
    public class MetadataImage : HideInspector
    {
    }

    [CustomEditor(typeof(Album))]
    public class MetadataAlbum : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Atlas))]
    public class CoreAtlas : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Image))]
    public class CoreImage : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Subscription))]
    public class CoreImageSubscription : HideInspector
    {
    }

    [CustomEditor(typeof(Runtime.Core.Prefabs))]
    public class CorePrefabs : HideInspector
    {
    }
}

#endif