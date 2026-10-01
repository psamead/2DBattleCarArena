using BattleCarArena.Battle;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BattleCarArena.Battle.Editor
{
    public static class BattleArenaTemplateBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/BattleArena.unity";
        private const string ArenaBackgroundPath = "Assets/_Project/Art/BattleArena/battle_arena_background_open_floor.png";
        private const string HealthBarSpritePath = "Assets/_Project/Art/BattleArena/HealthBarSquare.png";
        private const string BattleMusicPath = "Assets/_Project/Audio/Music/Locked_At_Redline_BattleLoop.wav";
        private static readonly Color SkyColor = new(0.42f, 0.47f, 0.52f, 1f);
        private static readonly Color RoadColor = new(0.28f, 0.3f, 0.32f, 1f);
        private static readonly Color WallColor = new(0.48f, 0.5f, 0.52f, 1f);

        [MenuItem("Tools/2D Battle Car Arena/Build Battle Arena Blockout")]
        public static void BuildScene()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            if (scene.rootCount > 0 && !EditorUtility.DisplayDialog(
                    "Rebuild Battle Arena Blockout",
                    "This replaces every object in the BattleArena scene with the default blockout. Continue?",
                    "Rebuild",
                    "Cancel"))
            {
                return;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Object.DestroyImmediate(root);
            }

            Sprite square = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (square == null)
            {
                Debug.LogError("Unity's built-in UISprite resource was not found. Battle Arena blockout was not created.");
                return;
            }

            Camera camera = CreateCamera();
            Sprite arenaBackground = AssetDatabase.LoadAssetAtPath<Sprite>(ArenaBackgroundPath);
            CreateBackdrop(square, arenaBackground);
            CreateWalls(square);

            GameObject controllerObject = new("BattleArenaController");
            BattleArenaController controller = controllerObject.AddComponent<BattleArenaController>();
            AudioSource battleMusic = controllerObject.AddComponent<AudioSource>();
            battleMusic.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(BattleMusicPath);
            battleMusic.loop = true;
            battleMusic.playOnAwake = false;
            battleMusic.spatialBlend = 0f;
            battleMusic.volume = 0.55f;

            (CarMotor2D playerMotor, CrashReporter playerCrash) = CreateCar(
                "PlayerCar", new Vector2(-4.5f, -1.77f), "Assets/_Project/Prefabs/Battle/ArmoredCarVisual.prefab");
            (CarMotor2D challengerMotor, CrashReporter challengerCrash) = CreateCar(
                "ChallengerCar", new Vector2(4.5f, -1.65f), "Assets/_Project/Prefabs/Battle/ChallengerCarVisual.prefab");

            (BattleHudView hud, BattleResultView resultView) = CreateHud(LoadHealthBarSprite(square));
            BattleArenaCameraRig cameraRig = camera.GetComponent<BattleArenaCameraRig>();
            SetCameraTargets(cameraRig, playerMotor.transform, challengerMotor.transform);
            SetControllerReferences(controller, playerMotor, challengerMotor, playerCrash, challengerCrash, hud, cameraRig);
            SetHudReferences(hud, resultView);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Created the Battle Arena blockout at " + ScenePath);
        }

        private static Sprite LoadHealthBarSprite(Sprite fallback)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(HealthBarSpritePath);
            if (sprite != null) return sprite;
            string absolutePath = System.IO.Path.GetFullPath(HealthBarSpritePath);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(absolutePath));
            Texture2D pixel = new(2, 2, TextureFormat.RGBA32, false);
            pixel.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            pixel.Apply();
            System.IO.File.WriteAllBytes(absolutePath, pixel.EncodeToPNG());
            Object.DestroyImmediate(pixel);
            AssetDatabase.ImportAsset(HealthBarSpritePath, ImportAssetOptions.ForceSynchronousImport);
            TextureImporter importer = AssetImporter.GetAtPath(HealthBarSpritePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(HealthBarSpritePath) ?? fallback;
        }

        private static Camera CreateCamera()
        {
            GameObject cameraObject = new("Main Camera");
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5.4f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = SkyColor;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.AddComponent<BattleArenaCameraRig>();
            return camera;
        }

        private static void CreateBackdrop(Sprite square, Sprite arenaBackground)
        {
            if (arenaBackground != null)
            {
                GameObject background = new("BattleArenaBackground");
                background.transform.position = new Vector3(0f, 0f, 1f);
                // Overscan the arena art so camera tracking and zooming never expose the clear color.
                background.transform.localScale = Vector3.one * 1.28f;
                SpriteRenderer renderer = background.AddComponent<SpriteRenderer>();
                renderer.sprite = arenaBackground;
                renderer.sortingOrder = -10;
            }
            else
            {
                CreateWorldBlock(square, "Background", null, new Vector3(0f, 1.7f, 5f), new Vector2(20f, 7.4f), SkyColor, 0);
            }

            CreateWorldBlock(square, "ForegroundRoad", null, new Vector3(0f, -3.65f, 0f), new Vector2(20f, 2.1f), RoadColor, 1);
            CreateWorldBlock(square, "RoadCenterMarking", null, new Vector3(0f, -3.61f, -0.1f), new Vector2(17.2f, 0.035f), new Color(0.72f, 0.72f, 0.69f), 2);
        }

        private static void CreateWalls(Sprite square)
        {
            CreateWall(square, "LeftBoundary", -7.75f);
            CreateWall(square, "RightBoundary", 7.75f);
        }

        private static void CreateWall(Sprite square, string name, float x)
        {
            GameObject wall = CreateWorldBlock(square, name, null, new Vector3(x, -0.28f, -0.25f), new Vector2(0.48f, 4.74f), WallColor, 4);
            BoxCollider2D wallCollider = wall.AddComponent<BoxCollider2D>();
            wallCollider.size = Vector2.one;
            wallCollider.isTrigger = false;
            wall.AddComponent<BattleArenaBoundary>();
        }

        private static (CarMotor2D motor, CrashReporter crash) CreateCar(
            string name, Vector2 position, string visualPrefabPath)
        {
            GameObject car = new(name);
            car.transform.position = new Vector3(position.x, position.y, -0.4f);

            Rigidbody2D body = car.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.linearDamping = 1.25f;
            body.angularDamping = 0f;
            body.mass = 1.5f;
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            BoxCollider2D collider = car.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(4.3f, 1.2f);
            collider.offset = new Vector2(0f, -0.05f);

            GameObject visualPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(visualPrefabPath);
            if (visualPrefab == null)
            {
                Debug.LogError("Car visual prefab not found: " + visualPrefabPath);
                Object.DestroyImmediate(car);
                return (null, null);
            }

            GameObject visual = PrefabUtility.InstantiatePrefab(visualPrefab) as GameObject;
            visual.transform.SetParent(car.transform, false);

            CarMotor2D motor = car.AddComponent<CarMotor2D>();
            SerializedObject motorObject = new(motor);
            motorObject.FindProperty("body").objectReferenceValue = body;
            motorObject.FindProperty("forcePerEnginePoint").floatValue = 0.18f;
            motorObject.ApplyModifiedPropertiesWithoutUndo();
            CrashReporter crash = car.AddComponent<CrashReporter>();
            return (motor, crash);
        }

        private static (BattleHudView hud, BattleResultView result) CreateHud(Sprite square)
        {
            GameObject canvasObject = new("BattleHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 5f;
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            TMP_Text playerName = CreateText("PlayerName", canvasObject.transform, font, 36f, Vector2.zero, new Vector2(520f, 48f), TextAlignmentOptions.MidlineLeft);
            TMP_Text challengerName = CreateText("ChallengerName", canvasObject.transform, font, 36f, Vector2.zero, new Vector2(520f, 48f), TextAlignmentOptions.MidlineRight);
            TMP_Text playerHealthLabel = CreateText("PlayerHealthValue", canvasObject.transform, font, 24f, Vector2.zero, new Vector2(520f, 36f), TextAlignmentOptions.MidlineLeft);
            TMP_Text challengerHealthLabel = CreateText("ChallengerHealthValue", canvasObject.transform, font, 24f, Vector2.zero, new Vector2(520f, 36f), TextAlignmentOptions.MidlineRight);
            ConfigureHudRect(playerName.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -82f), new Vector2(520f, 48f));
            ConfigureHudRect(challengerName.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-56f, -82f), new Vector2(520f, 48f));
            ConfigureHudRect(playerHealthLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(56f, -132f), new Vector2(520f, 36f));
            ConfigureHudRect(challengerHealthLabel.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-56f, -132f), new Vector2(520f, 36f));
            playerName.text = "PLAYER";
            challengerName.text = "CHALLENGER";
            playerHealthLabel.text = "65 / 65";
            challengerHealthLabel.text = "140 / 140";
            Image playerHealthFill = CreateHealthBar(square, canvasObject.transform, "PlayerHealthBar", true, new Color(0.32f, 0.78f, 0.44f));
            Image challengerHealthFill = CreateHealthBar(square, canvasObject.transform, "ChallengerHealthBar", false, new Color(0.88f, 0.42f, 0.3f));
            TMP_Text cue = CreateText("StartCue", canvasObject.transform, font, 54f, Vector2.zero, new Vector2(520f, 82f), TextAlignmentOptions.Center);
            ConfigureHudRect(cue.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -44f), new Vector2(520f, 82f));
            cue.text = "GET READY";

            GameObject resultPanel = CreateUiImage(square, "ResultPanel", canvasObject.transform, new Vector2(0f, 10f), new Vector2(820f, 300f), new Color(0.1f, 0.12f, 0.14f, 0.94f));
            TMP_Text resultText = CreateText("ResultText", resultPanel.transform, font, 64f, new Vector2(0f, 42f), new Vector2(780f, 112f), TextAlignmentOptions.Center);
            TMP_Text reasonText = CreateText("ResultReason", resultPanel.transform, font, 32f, new Vector2(0f, -65f), new Vector2(780f, 72f), TextAlignmentOptions.Center);
            resultPanel.SetActive(false);

            GameObject hudObject = new("BattleHudView");
            hudObject.transform.SetParent(canvasObject.transform, false);
            BattleHudView hud = hudObject.AddComponent<BattleHudView>();
            SerializedObject hudSerialized = new(hud);
            hudSerialized.FindProperty("playerNameText").objectReferenceValue = playerName;
            hudSerialized.FindProperty("challengerNameText").objectReferenceValue = challengerName;
            hudSerialized.FindProperty("playerHealthText").objectReferenceValue = playerHealthLabel;
            hudSerialized.FindProperty("challengerHealthText").objectReferenceValue = challengerHealthLabel;
            hudSerialized.FindProperty("playerHealthFill").objectReferenceValue = playerHealthFill;
            hudSerialized.FindProperty("challengerHealthFill").objectReferenceValue = challengerHealthFill;
            hudSerialized.FindProperty("startCueText").objectReferenceValue = cue;
            hudSerialized.ApplyModifiedPropertiesWithoutUndo();

            BattleResultView result = hudObject.AddComponent<BattleResultView>();
            SerializedObject resultSerialized = new(result);
            resultSerialized.FindProperty("resultPanel").objectReferenceValue = resultPanel;
            resultSerialized.FindProperty("resultText").objectReferenceValue = resultText;
            resultSerialized.FindProperty("reasonText").objectReferenceValue = reasonText;
            resultSerialized.ApplyModifiedPropertiesWithoutUndo();
            return (hud, result);
        }

        private static Image CreateHealthBar(Sprite square, Transform parent, string name, bool fillFromLeft, Color fillColor)
        {
            GameObject track = CreateUiImage(square, name, parent, Vector2.zero, new Vector2(570f, 26f), new Color(0.18f, 0.19f, 0.2f, 0.9f));
            RectTransform trackRect = track.GetComponent<RectTransform>();
            Vector2 topAnchor = fillFromLeft ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
            Vector2 topPivot = topAnchor;
            Vector2 offset = fillFromLeft ? new Vector2(56f, -46f) : new Vector2(-56f, -46f);
            ConfigureHudRect(trackRect, topAnchor, topPivot, offset, new Vector2(570f, 26f));
            RectTransform fillRect = CreateRect("Fill", track.transform);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.pivot = fillFromLeft ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            Image fill = fillRect.gameObject.AddComponent<Image>();
            fill.sprite = square;
            fill.color = fillColor;
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillOrigin = fillFromLeft ? 0 : 1;
            fill.fillAmount = 1f;
            fill.raycastTarget = false;
            return fill;
        }

        private static GameObject CreateUiImage(Sprite square, string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = square;
            image.color = color;
            image.raycastTarget = false;
            return rect.gameObject;
        }

        private static TMP_Text CreateText(string name, Transform parent, TMP_FontAsset font, float fontSize, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
        {
            RectTransform rect = CreateRect(name, parent);
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = font;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.alignment = alignment;
            text.text = string.Empty;
            text.raycastTarget = false;
            return text;
        }

        private static void ConfigureHudRect(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static GameObject CreateWorldBlock(Sprite square, string name, Transform parent, Vector3 position, Vector2 size, Color color, int sortingOrder)
        {
            GameObject block = new(name);
            if (parent != null)
            {
                block.transform.SetParent(parent, false);
            }

            block.transform.localPosition = position;
            block.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
            renderer.sprite = square;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return block;
        }

        private static RectTransform CreateRect(string name, Transform parent)
        {
            GameObject child = new(name, typeof(RectTransform));
            RectTransform rect = child.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            return rect;
        }

        private static void SetControllerReferences(
            BattleArenaController controller,
            CarMotor2D playerMotor,
            CarMotor2D challengerMotor,
            CrashReporter playerCrash,
            CrashReporter challengerCrash,
            BattleHudView hud,
            BattleArenaCameraRig cameraRig)
        {
            SerializedObject serialized = new(controller);
            serialized.FindProperty("playerMotor").objectReferenceValue = playerMotor;
            serialized.FindProperty("challengerMotor").objectReferenceValue = challengerMotor;
            serialized.FindProperty("playerCrashReporter").objectReferenceValue = playerCrash;
            serialized.FindProperty("challengerCrashReporter").objectReferenceValue = challengerCrash;
            serialized.FindProperty("hud").objectReferenceValue = hud;
            serialized.FindProperty("cameraRig").objectReferenceValue = cameraRig;
            serialized.FindProperty("challengerEnginePower").intValue = 220;
            serialized.FindProperty("challengerWeaponDamage").intValue = 6;
            serialized.FindProperty("challengerArmorDurability").intValue = 140;
            serialized.FindProperty("contactDamageInterval").floatValue = 5.5f;
            serialized.FindProperty("battleDurationSeconds").floatValue = 60f;
            serialized.FindProperty("boundaryCrashDamage").intValue = 2;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetCameraTargets(BattleArenaCameraRig rig, Transform player, Transform challenger)
        {
            SerializedObject serialized = new(rig);
            serialized.FindProperty("targetCamera").objectReferenceValue = rig.GetComponent<Camera>();
            serialized.FindProperty("playerTarget").objectReferenceValue = player;
            serialized.FindProperty("challengerTarget").objectReferenceValue = challenger;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetHudReferences(BattleHudView hud, BattleResultView resultView)
        {
            SerializedObject serialized = new(hud);
            serialized.FindProperty("resultView").objectReferenceValue = resultView;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
