#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleCarArena.UI.Editor
{
    internal static class StartMenuTemplateBuilder
    {
        private const string ScenePath = "Assets/Scenes/StartMenu.unity";
        private const string ThemeFolder = "Assets/_Project/Data/UI";
        private const string ThemePath = ThemeFolder + "/DefaultStartMenuTheme.asset";

        [MenuItem("Tools/2D Battle Car Arena/Build Start Menu Template")]
        private static void BuildTemplate()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != ScenePath)
            {
                EditorUtility.DisplayDialog("Start Menu Template", $"Open {ScenePath} before building the template.", "OK");
                return;
            }

            GameObject existing = GameObject.Find("StartMenuTemplate");
            if (existing != null)
            {
                bool replace = EditorUtility.DisplayDialog("Replace Start Menu Template?", "A StartMenuTemplate already exists. Replace it with a fresh template?", "Replace", "Cancel");
                if (!replace)
                {
                    return;
                }

                Undo.DestroyObjectImmediate(existing);
            }

            StartMenuTheme theme = GetOrCreateTheme();
            GameObject root = new("StartMenuTemplate");
            Undo.RegisterCreatedObjectUndo(root, "Build Start Menu Template");
            SceneManager.MoveGameObjectToScene(root, activeScene);
            StartMenuTemplateFactory.Build(root, theme);
            EditorSceneManager.MarkSceneDirty(activeScene);
            EditorSceneManager.SaveScene(activeScene);
            Selection.activeGameObject = root;
            Debug.Log($"Built Start Menu template and saved {ScenePath}.", root);
        }

        private static StartMenuTheme GetOrCreateTheme()
        {
            StartMenuTheme theme = AssetDatabase.LoadAssetAtPath<StartMenuTheme>(ThemePath);
            if (theme != null)
            {
                return theme;
            }

            EnsureFolder("Assets/_Project", "Data");
            EnsureFolder("Assets/_Project/Data", "UI");
            theme = ScriptableObject.CreateInstance<StartMenuTheme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
            AssetDatabase.SaveAssets();
            return theme;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
