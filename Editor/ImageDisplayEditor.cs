#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using UriAlbum.Runtime.Display;

namespace UriAlbum.Editor
{
    [CustomEditor(typeof(ImageDisplay))]
    public class ImageDisplayEditor : UnityEditor.Editor
    {
        private void OnEnable()
        {
            EditorApplication.hierarchyChanged += EditorUtil.UpdateAlbumsInScene;
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyChanged -= EditorUtil.UpdateAlbumsInScene;
        }

        public override void OnInspectorGUI()
        {
            EditorUtil.DrawHeader();

            if (EditorApplication.isPlaying) return;

            var imageDisplay = (ImageDisplay) target;

            var useTag = imageDisplay._useTag;

            EditorGUILayout.BeginHorizontal();

            var activeStyle = new GUIStyle(GUI.skin.button) {normal = {textColor = Color.green}};

            if (GUILayout.Button("Use Tag", useTag ? activeStyle : GUI.skin.button))
            {
                useTag = !useTag;
            }

            imageDisplay._useTag = useTag;
            EditorGUILayout.EndHorizontal();

            if (useTag) imageDisplay._tag = EditorGUILayout.TextField("Tag", imageDisplay._tag);

            imageDisplay._album = EditorUtil.AlbumSelector("Album", imageDisplay._album);

            serializedObject.ApplyModifiedProperties();

            EditorUtil.DrawFooter();
        }
    }
}

#endif