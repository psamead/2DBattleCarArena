using TMPro;
using UnityEngine;

namespace BattleCarArena.UI
{
    public static class StartMenuTemplateFactory
    {
        private static readonly Color DefaultBackground = new(0.035f, 0.045f, 0.065f, 1f);
        private static readonly Color DefaultPanel = new(0.04f, 0.055f, 0.08f, 0.88f);
        private static readonly Color DefaultAccent = new(1f, 0.34f, 0.08f, 1f);
        private static readonly Color DefaultText = new(0.96f, 0.97f, 1f, 1f);
        private static readonly Color DefaultButton = new(0.12f, 0.16f, 0.23f, 1f);

        public static StartMenuController Build(GameObject root, StartMenuTheme theme)
        {
            root.name = "StartMenuTemplate";

            StartMenuView view = GetOrAdd<StartMenuView>(root);
            AudioSource audioSource = GetOrAdd<AudioSource>(root);
            audioSource.loop = true;
            audioSource.playOnAwake = false;

            GameObject canvasObject = CreateUiObject("Canvas", root.transform);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            UnityEngine.UI.CanvasScaler scaler = canvasObject.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Full-screen sprite slot. Assign a Sprite on the theme asset to replace it later.
            UnityEngine.UI.Image background = CreateImage("BackgroundPanel", canvasObject.transform, DefaultBackground);
            Stretch(background.rectTransform);
            background.raycastTarget = false;

            UnityEngine.UI.Image tint = CreateImage("ReadabilityTint", canvasObject.transform, new Color(0f, 0f, 0f, 0.25f));
            Stretch(tint.rectTransform);
            tint.raycastTarget = false;

            UnityEngine.UI.Image menuPanel = CreateImage("MenuPanel", canvasObject.transform, DefaultPanel);
            SetRect(menuPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f), new Vector2(-310f, 0f), new Vector2(620f, 520f));
            menuPanel.raycastTarget = false;

            GameObject content = CreateUiObject("Content", menuPanel.transform);
            Stretch((RectTransform)content.transform, 56f, 56f, 48f, 48f);

            TMP_Text title = CreateText("Title", content.transform, "BATTLE CAR ARENA", 62f, DefaultText, FontStyles.Bold);
            title.enableAutoSizing = true;
            title.fontSizeMin = 34f;
            title.fontSizeMax = 62f;
            SetRect(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 110f));

            TMP_Text subtitle = CreateText("Subtitle", content.transform, "SELECT AN OPTION", 20f, DefaultAccent, FontStyles.Bold);
            subtitle.characterSpacing = 4f;
            SetRect(subtitle.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(0f, 42f));

            GameObject selectionBars = CreateUiObject("SelectionBars", content.transform);
            SetRect((RectTransform)selectionBars.transform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(0f, 206f));
            UnityEngine.UI.VerticalLayoutGroup layout = selectionBars.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
            layout.spacing = 18f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = false;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            (UnityEngine.UI.Button startButton, TMP_Text startLabel) = CreateSelectionBar("StartGameBar", selectionBars.transform, "START GAME");
            (UnityEngine.UI.Button exitButton, TMP_Text exitLabel) = CreateSelectionBar("QuitGameBar", selectionBars.transform, "QUIT GAME");
            SetNavigation(startButton, exitButton, exitButton);
            SetNavigation(exitButton, startButton, startButton);

            EnsureEventSystem(root.transform);

            view.Configure(background, null, menuPanel, null, startButton, exitButton, title, subtitle, startLabel, exitLabel, null, audioSource);
            view.ApplyTheme(theme);

            StartMenuController controller = GetOrAdd<StartMenuController>(root);
            controller.Configure(view, theme, "GarageHub");
            return controller;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject result = new(name, typeof(RectTransform));
            result.transform.SetParent(parent, false);
            return result;
        }

        private static UnityEngine.UI.Image CreateImage(string name, Transform parent, Color color)
        {
            GameObject target = CreateUiObject(name, parent);
            UnityEngine.UI.Image image = target.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text CreateText(string name, Transform parent, string value, float size, Color color, FontStyles style)
        {
            TextMeshProUGUI text = CreateUiObject(name, parent).AddComponent<TextMeshProUGUI>();
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Left;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.raycastTarget = false;
            return text;
        }

        private static (UnityEngine.UI.Button button, TMP_Text label) CreateSelectionBar(string name, Transform parent, string label)
        {
            UnityEngine.UI.Image image = CreateImage(name, parent, DefaultButton);
            image.raycastTarget = true;

            UnityEngine.UI.LayoutElement layout = image.gameObject.AddComponent<UnityEngine.UI.LayoutElement>();
            layout.minHeight = 88f;
            layout.preferredHeight = 88f;

            UnityEngine.UI.Button button = image.gameObject.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;

            UnityEngine.UI.Image marker = CreateImage("SelectionMarker", image.transform, DefaultAccent);
            marker.raycastTarget = false;
            SetRect(marker.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f), Vector2.zero, new Vector2(8f, 0f));

            TMP_Text text = CreateText("Label", image.transform, label, 27f, DefaultText, FontStyles.Bold);
            text.alignment = TextAlignmentOptions.Center;
            text.characterSpacing = 4f;
            Stretch(text.rectTransform, 24f, 24f, 8f, 8f);
            return (button, text);
        }

        private static void SetNavigation(UnityEngine.UI.Selectable selectable, UnityEngine.UI.Selectable up, UnityEngine.UI.Selectable down)
        {
            UnityEngine.UI.Navigation navigation = selectable.navigation;
            navigation.mode = UnityEngine.UI.Navigation.Mode.Explicit;
            navigation.selectOnUp = up;
            navigation.selectOnDown = down;
            selectable.navigation = navigation;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            if (Object.FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new("EventSystem", typeof(UnityEngine.EventSystems.EventSystem));
            eventSystemObject.transform.SetParent(parent, false);
            UnityEngine.InputSystem.UI.InputSystemUIInputModule inputModule = eventSystemObject.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            inputModule.AssignDefaultActions();
        }

        private static void Stretch(RectTransform target, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            target.anchorMin = Vector2.zero;
            target.anchorMax = Vector2.one;
            target.pivot = new Vector2(0.5f, 0.5f);
            target.offsetMin = new Vector2(left, bottom);
            target.offsetMax = new Vector2(-right, -top);
        }

        private static void SetRect(RectTransform target, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            target.anchorMin = anchorMin;
            target.anchorMax = anchorMax;
            target.pivot = pivot;
            target.anchoredPosition = anchoredPosition;
            target.sizeDelta = sizeDelta;
        }
    }
}
