#if UNITY_EDITOR

using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using URIAlbum.Runtime.Core;
using URIAlbum.Runtime.Display;

namespace URIAlbum.Editor
{
    public static class EditorUtil
    {
        private const string LogoGUID = "1f18eac734aafe547b9320642cb4187b";
        private const string LanguagePrefKey = "URIAlbum.Editor.Language";
        public const string BaseURL = "https://urialbum.com";
        public static string APIUrl => $"{BaseURL}/api/world";
        public const int PreparedAtlasUrls = 1024;

        public static GUIStyle HeaderStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleCenter
        };

        public static GUIStyle BoxStyle = new GUIStyle("box")
        {
            padding = new RectOffset(10, 10, 16, 16)
        };

        private static Texture2D logoTexture;
        private static float LogoAspectRatio => logoTexture != null ? (float)logoTexture.width / logoTexture.height : 4f;

        public enum EditorLanguage
        {
            English = 0,
            Japanese = 1,
            Korean = 2
        }

        private static EditorLanguage currentLanguage = EditorLanguage.English;

        private static readonly Dictionary<string, string> EnglishText = new Dictionary<string, string>
        {
            { "AlbumSelection", "Album Selection" },
            { "Album", "Album" },
            { "NoAlbumFound", "No album found." },
            { "RefreshAlbumSetting", "Refresh" },
            { "PlayModeDisabled", "Editing is disabled in Play Mode." },
            { "Configuration", "Configuration" },
            { "Group", "Group" },
            { "Albums", "Albums" },
            { "Reset", "Reset" },
            { "Refresh", "Refresh" },
            { "LinkKey", "Link Key" },
            { "Configure", "Configure" },
            { "NoAlbumInGroup", "No album in group." },
            { "Loading", "Loading..." },
            { "Language", "Language" },
            { "ImageDisplaySetting", "Image Display Setting" },
            { "AlbumSetting", "Album Setting" },
            { "Tag", "Tag" },
            { "TagPreset", "Preset" },
            { "Custom", "Custom" },
            { "None", "None" }
        };

        private static readonly Dictionary<string, string> JapaneseText = new Dictionary<string, string>
        {
            { "AlbumSelection", "アルバム選択" },
            { "Album", "アルバム" },
            { "NoAlbumFound", "アルバムが見つかりません。" },
            { "RefreshAlbumSetting", "更新" },
            { "PlayModeDisabled", "再生中は編集できません。" },
            { "Configuration", "設定" },
            { "Group", "グループ" },
            { "Albums", "アルバム一覧" },
            { "Reset", "リセット" },
            { "Refresh", "更新" },
            { "LinkKey", "リンクキー" },
            { "Configure", "設定" },
            { "NoAlbumInGroup", "グループ内にアルバムがありません。" },
            { "Loading", "読み込み中..." },
            { "Language", "言語" },
            { "ImageDisplaySetting", "Image Display 設定" },
            { "AlbumSetting", "アルバム設定" },
            { "Tag", "タグ" },
            { "TagPreset", "プリセット" },
            { "Custom", "カスタム" },
            { "None", "なし" }
        };

        private static readonly Dictionary<string, string> KoreanText = new Dictionary<string, string>
        {
            { "AlbumSelection", "앨범 선택" },
            { "Album", "앨범" },
            { "NoAlbumFound", "앨범을 찾을 수 없습니다." },
            { "RefreshAlbumSetting", "새로고침" },
            { "PlayModeDisabled", "재생 중에는 편집할 수 없습니다." },
            { "Configuration", "설정" },
            { "Group", "그룹" },
            { "Albums", "앨범 목록" },
            { "Reset", "초기화" },
            { "Refresh", "새로고침" },
            { "LinkKey", "링크 키" },
            { "Configure", "설정" },
            { "NoAlbumInGroup", "그룹에 앨범이 없습니다." },
            { "Loading", "불러오는 중..." },
            { "Language", "언어" },
            { "ImageDisplaySetting", "Image Display 설정" },
            { "AlbumSetting", "앨범 설정" },
            { "Tag", "태그" },
            { "TagPreset", "프리셋" },
            { "Custom", "직접 입력" },
            { "None", "없음" }
        };

        static EditorUtil()
        {
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(LogoGUID));
            currentLanguage = (EditorLanguage)EditorPrefs.GetInt(LanguagePrefKey, (int)EditorLanguage.English);
        }

        private static void OnHierarchyChanged()
        {
            UpdateImageDisplayOrders();
        }

        private static T[] SortByHierarchyOrder<T>(T[] objects) where T : Component
        {
            return objects.OrderBy(GetHierarchyPath).ToArray();
        }

        private static string GetHierarchyPath<T>(T obj) where T : Component
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

        private static void UpdateImageDisplayOrders()
        {
            var imageDisplays = Object.FindObjectsOfType<ImageDisplay>(true);
            var sortedDisplays = SortByHierarchyOrder(imageDisplays);
            for (var i = 0; i < sortedDisplays.Length; i++)
            {
                var display = sortedDisplays[i];
                display.order = i;
            }
        }

        public static void DrawHeader()
        {
            var height = 50;
            var width = height * LogoAspectRatio;
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(logoTexture, GUILayout.Width(width), GUILayout.Height(height));
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(16);
        }

        public static void DrawFooter()
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(Tr("Language"), GUILayout.Width(70));
            var options = new[] { "English", "日本語", "한국어" };
            var selected = (EditorLanguage)EditorGUILayout.Popup((int)currentLanguage, options, GUILayout.MaxWidth(120));
            if (selected != currentLanguage)
            {
                currentLanguage = selected;
                EditorPrefs.SetInt(LanguagePrefKey, (int)currentLanguage);
            }
            EditorGUILayout.EndHorizontal();
        }

        public static T CreateObject<T>(string objectName, GameObject parent) where T : Component
        {
            var instance = new GameObject(objectName);
            instance.transform.SetParent(parent.transform);
            return instance.AddComponent<T>();
        }

        public static string Tr(string key)
        {
            Dictionary<string, string> dict;
            switch (currentLanguage)
            {
                case EditorLanguage.Japanese:
                    dict = JapaneseText;
                    break;
                case EditorLanguage.Korean:
                    dict = KoreanText;
                    break;
                default:
                    dict = EnglishText;
                    break;
            }
            if (dict.TryGetValue(key, out var value)) return value;
            if (EnglishText.TryGetValue(key, out var fallback)) return fallback;
            return key;
        }

    }
}

#endif
