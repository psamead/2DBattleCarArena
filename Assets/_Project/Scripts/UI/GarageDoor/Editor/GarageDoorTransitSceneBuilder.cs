#if UNITY_EDITOR
using System.Collections.Generic;
using BattleCarArena.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleCarArena.UI.Editor
{
    internal static class GarageDoorTransitSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/GarageTransit.unity";
        private const string ThemeFolder = "Assets/_Project/Data/UI/GarageDoor";
        private const string ThemePath = ThemeFolder + "/DefaultGarageDoorTransitionTheme.asset";
        private const string GarageThemePath = "Assets/_Project/Data/UI/GarageHub/DefaultGarageHubTheme.asset";
        private const string FramePath = "Assets/_Project/Art/UI/GarageDoor/GarageDoorFrame.png";
        private const string PanelPath = "Assets/_Project/Art/UI/GarageDoor/GarageDoorPanel.png";

        [MenuItem("Tools/2D Battle Car Arena/Build Garage Door Transit Scene")]
        private static void BuildSceneFromMenu()
        {
            BuildScene();
        }

        public static void BuildScene()
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogError("Stop Play Mode before building the GarageTransit scene.");
                return;
            }

            EnsureSpriteImporter(FramePath);
            EnsureSpriteImporter(PanelPath);
            AssetDatabase.Refresh();

            Sprite frame = AssetDatabase.LoadAssetAtPath<Sprite>(FramePath);
            Sprite panel = AssetDatabase.LoadAssetAtPath<Sprite>(PanelPath);
            GarageHubTheme garageTheme = AssetDatabase.LoadAssetAtPath<GarageHubTheme>(GarageThemePath);
            if (frame == null || panel == null || garageTheme == null)
            {
                Debug.LogError("Garage door sprites or the Garage Hub theme are missing.");
                return;
            }

            GarageDoorTransitionTheme theme = GetOrCreateTheme();
            theme.Configure(frame, panel, garageTheme.NeutralBackground, garageTheme.CarPreview);
            EditorUtility.SetDirty(theme);
            AssetDatabase.SaveAssets();

            Scene originalActiveScene = SceneManager.GetActiveScene();
            Scene transitScene = SceneManager.GetSceneByPath(ScenePath);
            bool openedByBuilder = !transitScene.IsValid() || !transitScene.isLoaded;
            if (openedByBuilder)
            {
                transitScene = System.IO.File.Exists(ScenePath)
                    ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive)
                    : EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            }

            EditorSceneManager.SetActiveScene(transitScene);
            foreach (GameObject oldRoot in transitScene.GetRootGameObjects())
            {
                Undo.DestroyObjectImmediate(oldRoot);
            }

            BuildHierarchy(transitScene, theme);
            EditorSceneManager.MarkSceneDirty(transitScene);
            EditorSceneManager.SaveScene(transitScene, ScenePath);
            AddSceneToBuildSettings();

            if (openedByBuilder)
            {
                if (originalActiveScene.IsValid() && originalActiveScene.isLoaded)
                {
                    EditorSceneManager.SetActiveScene(originalActiveScene);
                }

                EditorSceneManager.CloseScene(transitScene, true);
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Built GarageTransit and enabled it in Build Settings: {ScenePath}");
        }

        private static void BuildHierarchy(Scene scene, GarageDoorTransitionTheme theme)
        {
            GameObject root = new("GarageDoorTransit");
            SceneManager.MoveGameObjectToScene(root, scene);
            GarageDoorTransitController controller = root.AddComponent<GarageDoorTransitController>();

            GameObject canvasObject = CreateUiObject("Canvas", root.transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            UnityEngine.UI.Image background = CreateImage("GarageInterior", canvasObject.transform, Color.white);
            Stretch(background.rectTransform);
            background.sprite = theme.GarageBackgroundSprite;
            background.preserveAspect = false;

            UnityEngine.UI.Image car = CreateImage("GarageCar", canvasObject.transform, Color.white);
            SetRect(car.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -10f), new Vector2(590f, 390f));
            car.sprite = theme.GarageCarSprite;
            car.preserveAspect = true;
            car.gameObject.SetActive(theme.GarageCarSprite != null);

            UnityEngine.UI.Image darkness = CreateImage("InteriorDarkness", canvasObject.transform, new Color(0f, 0f, 0f, 0.58f));
            Stretch(darkness.rectTransform);
            darkness.raycastTarget = false;

            UnityEngine.UI.Image panel = CreateImage("GarageDoorPanel", canvasObject.transform, Color.white);
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -75f), new Vector2(940f, 568f));
            panel.sprite = theme.PanelSprite;
            panel.preserveAspect = true;
            panel.raycastTarget = false;

            UnityEngine.UI.Image frame = CreateImage("GarageDoorFrame", canvasObject.transform, Color.white);
            SetRect(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180f, 827f));
            frame.sprite = theme.FrameSprite;
            frame.preserveAspect = true;
            frame.raycastTarget = false;

            controller.Configure(theme, background, car, darkness, panel, frame);
        }

        private static GarageDoorTransitionTheme GetOrCreateTheme()
        {
            EnsureFolder("Assets/_Project/Data", "UI");
            EnsureFolder("Assets/_Project/Data/UI", "GarageDoor");

            GarageDoorTransitionTheme theme = AssetDatabase.LoadAssetAtPath<GarageDoorTransitionTheme>(ThemePath);
            if (theme != null)
            {
                return theme;
            }

            theme = ScriptableObject.CreateInstance<GarageDoorTransitionTheme>();
            AssetDatabase.CreateAsset(theme, ThemePath);
            return theme;
        }

        private static void EnsureSpriteImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = 100f;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        private static void AddSceneToBuildSettings()
        {
            List<EditorBuildSettingsScene> scenes = new(EditorBuildSettings.scenes);
            for (int i = 0; i < scenes.Count; i++)
            {
                if (scenes[i].path == ScenePath)
                {
                    scenes[i] = new EditorBuildSettingsScene(ScenePath, true);
                    EditorBuildSettings.scenes = scenes.ToArray();
                    return;
                }
            }

            scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = parent + "/" + child;
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static UnityEngine.UI.Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject gameObject = CreateUiObject(name, parent);
            UnityEngine.UI.Image image = gameObject.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            return image;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
#endif
