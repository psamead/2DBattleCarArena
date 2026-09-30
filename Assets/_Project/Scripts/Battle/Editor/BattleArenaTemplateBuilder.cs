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

            CreateCamera();
            CreateBackdrop(square);
            CreateWalls(square);

            GameObject controllerObject = new("BattleArenaController");
            BattleArenaController controller = controllerObject.AddComponent<BattleArenaController>();

            (CarMotor2D playerMotor, CrashReporter playerCrash) = CreateCar(
                square, "PlayerCar", new Vector2(-4.5f, -2.08f), new Color(0.27f, 0.62f, 0.78f), true);
            (CarMotor2D challengerMotor, CrashReporter challengerCrash) = CreateCar(
                square, "ChallengerCar", new Vector2(4.5f, -2.08f), new Color(0.82f, 0.53f, 0.25f), false);

            (BattleHudView hud, BattleResultView resultView) = CreateHud(square);
            SetControllerReferences(controller, playerMotor, challengerMotor, playerCrash, challengerCrash, hud);
            SetHudReferences(hud, resultView);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Created the Battle Arena blockout at " + ScenePath);
        }

        private static void CreateCamera()
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
        }

        private static void CreateBackdrop(Sprite square)
        {
            CreateWorldBlock(square, "Background", null, new Vector3(0f, 1.7f, 5f), new Vector2(20f, 7.4f), SkyColor, 0);
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
            wallCollider.isTrigger = true;
            wall.AddComponent<BattleArenaBoundary>();
        }

        private static (CarMotor2D motor, CrashReporter crash) CreateCar(
            Sprite square, string name, Vector2 position, Color bodyColor, bool facingRight)
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
            collider.size = new Vector2(2.25f, 0.82f);

            float facingSign = facingRight ? 1f : -1f;
            CreateWorldBlock(square, "BodyBlock", car.transform, new Vector3(0f, 0f, 0f), new Vector2(2.25f, 0.78f), bodyColor, 6);
            CreateWorldBlock(square, "CabBlock", car.transform, new Vector3(-0.12f * facingSign, 0.48f, -0.02f), new Vector2(1.1f, 0.36f), Color.Lerp(bodyColor, Color.white, 0.22f), 7);
            CreateWorldBlock(square, "FrontBumperBlock", car.transform, new Vector3(1.18f * facingSign, -0.1f, -0.03f), new Vector2(0.22f, 0.28f), new Color(0.2f, 0.21f, 0.22f), 8);
            CreateWorldBlock(square, "FrontWheelBlock", car.transform, new Vector3(0.67f, -0.46f, -0.04f), new Vector2(0.42f, 0.32f), new Color(0.16f, 0.17f, 0.18f), 8);
            CreateWorldBlock(square, "RearWheelBlock", car.transform, new Vector3(-0.67f, -0.46f, -0.04f), new Vector2(0.42f, 0.32f), new Color(0.16f, 0.17f, 0.18f), 8);

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
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            TMP_FontAsset font = TMP_Settings.defaultFontAsset;
            TMP_Text playerName = CreateText("PlayerName", canvasObject.transform, font, 48f, new Vector2(-485f, -34f), new Vector2(520f, 68f), TextAlignmentOptions.MidlineLeft);
            TMP_Text challengerName = CreateText("ChallengerName", canvasObject.transform, font, 48f, new Vector2(485f, -34f), new Vector2(520f, 68f), TextAlignmentOptions.MidlineRight);
            TMP_Text playerHealthLabel = CreateText("PlayerHealthValue", canvasObject.transform, font, 28f, new Vector2(-485f, -144f), new Vector2(520f, 42f), TextAlignmentOptions.MidlineLeft);
            TMP_Text challengerHealthLabel = CreateText("ChallengerHealthValue", canvasObject.transform, font, 28f, new Vector2(485f, -144f), new Vector2(520f, 42f), TextAlignmentOptions.MidlineRight);
            Image playerHealthFill = CreateHealthBar(square, canvasObject.transform, "PlayerHealthBar", -485f, true, new Color(0.32f, 0.78f, 0.44f));
            Image challengerHealthFill = CreateHealthBar(square, canvasObject.transform, "ChallengerHealthBar", 485f, false, new Color(0.88f, 0.42f, 0.3f));
            TMP_Text cue = CreateText("StartCue", canvasObject.transform, font, 72f, new Vector2(0f, 185f), new Vector2(760f, 110f), TextAlignmentOptions.Center);

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

        private static Image CreateHealthBar(Sprite square, Transform parent, string name, float x, bool fillFromLeft, Color fillColor)
        {
            GameObject track = CreateUiImage(square, name, parent, new Vector2(x, -98f), new Vector2(620f, 30f), new Color(0.18f, 0.19f, 0.2f, 0.9f));
            RectTransform fillRect = CreateRect("Fill", track.transform);
            fillRect.anchorMin = fillFromLeft ? Vector2.zero : new Vector2(1f, 0f);
            fillRect.anchorMax = fillFromLeft ? Vector2.one : new Vector2(0f, 1f);
            fillRect.pivot = fillFromLeft ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
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
            BattleHudView hud)
        {
            SerializedObject serialized = new(controller);
            serialized.FindProperty("playerMotor").objectReferenceValue = playerMotor;
            serialized.FindProperty("challengerMotor").objectReferenceValue = challengerMotor;
            serialized.FindProperty("playerCrashReporter").objectReferenceValue = playerCrash;
            serialized.FindProperty("challengerCrashReporter").objectReferenceValue = challengerCrash;
            serialized.FindProperty("hud").objectReferenceValue = hud;
            serialized.FindProperty("challengerEnginePower").intValue = 220;
            serialized.FindProperty("challengerWeaponDamage").intValue = 15;
            serialized.FindProperty("challengerArmorDurability").intValue = 140;
            serialized.FindProperty("contactDamageInterval").floatValue = 0.65f;
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
