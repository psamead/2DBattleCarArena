using TMPro;
using UnityEngine;

namespace BattleCarArena.UI
{
    public sealed class StartMenuView : MonoBehaviour
    {
        [Header("Replaceable Visual Slots")]
        [SerializeField] private UnityEngine.UI.Image backgroundImage;
        [SerializeField] private UnityEngine.UI.Image logoImage;
        [SerializeField] private UnityEngine.UI.Image panelImage;
        [SerializeField] private UnityEngine.UI.Image accentImage;
        [SerializeField] private UnityEngine.UI.Image startButtonImage;
        [SerializeField] private UnityEngine.UI.Image exitButtonImage;

        [Header("Text")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text subtitleText;
        [SerializeField] private TMP_Text startButtonText;
        [SerializeField] private TMP_Text exitButtonText;
        [SerializeField] private TMP_Text footerText;

        [Header("Interaction")]
        [SerializeField] private UnityEngine.UI.Button startButton;
        [SerializeField] private UnityEngine.UI.Button exitButton;
        [SerializeField] private AudioSource musicSource;

        public UnityEngine.UI.Button StartButton => startButton;
        public UnityEngine.UI.Button ExitButton => exitButton;

        public void Configure(
            UnityEngine.UI.Image background,
            UnityEngine.UI.Image logo,
            UnityEngine.UI.Image panel,
            UnityEngine.UI.Image accent,
            UnityEngine.UI.Button start,
            UnityEngine.UI.Button exit,
            TMP_Text title,
            TMP_Text subtitle,
            TMP_Text startLabel,
            TMP_Text exitLabel,
            TMP_Text footer,
            AudioSource audioSource)
        {
            backgroundImage = background;
            logoImage = logo;
            panelImage = panel;
            accentImage = accent;
            startButton = start;
            exitButton = exit;
            startButtonImage = start != null ? start.targetGraphic as UnityEngine.UI.Image : null;
            exitButtonImage = exit != null ? exit.targetGraphic as UnityEngine.UI.Image : null;
            titleText = title;
            subtitleText = subtitle;
            startButtonText = startLabel;
            exitButtonText = exitLabel;
            footerText = footer;
            musicSource = audioSource;
        }

        public void ApplyTheme(StartMenuTheme theme)
        {
            if (theme == null)
            {
                if (logoImage != null)
                {
                    logoImage.gameObject.SetActive(false);
                }

                return;
            }

            ApplyImage(backgroundImage, theme.BackgroundSprite, theme.BackgroundColor);
            ApplyImage(panelImage, null, theme.PanelColor);
            ApplyImage(accentImage, null, theme.AccentColor);
            ApplyImage(startButtonImage, theme.ButtonSprite, theme.ButtonColor);
            ApplyImage(exitButtonImage, theme.ButtonSprite, theme.ButtonColor);

            if (logoImage != null)
            {
                logoImage.sprite = theme.LogoSprite;
                logoImage.color = Color.white;
                logoImage.preserveAspect = true;
                logoImage.gameObject.SetActive(theme.LogoSprite != null);
            }

            ApplyText(titleText, theme.FontAsset, theme.PrimaryTextColor);
            ApplyText(subtitleText, theme.FontAsset, theme.AccentColor);
            ApplyText(startButtonText, theme.FontAsset, theme.PrimaryTextColor);
            ApplyText(exitButtonText, theme.FontAsset, theme.PrimaryTextColor);
            ApplyText(footerText, theme.FontAsset, theme.MutedTextColor);

            ApplyButtonColors(startButton, theme);
            ApplyButtonColors(exitButton, theme);

            if (musicSource != null)
            {
                musicSource.clip = theme.MenuMusic;
            }
        }

        public void PlayMusic()
        {
            if (musicSource == null || musicSource.clip == null || musicSource.isPlaying)
            {
                return;
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.Play();
        }

        public void SelectPrimaryAction()
        {
            if (startButton != null && startButton.IsInteractable())
            {
                startButton.Select();
            }
        }

        private static void ApplyImage(UnityEngine.UI.Image image, Sprite sprite, Color color)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = color;
        }

        private static void ApplyText(TMP_Text text, TMP_FontAsset font, Color color)
        {
            if (text == null)
            {
                return;
            }

            if (font != null)
            {
                text.font = font;
            }

            text.color = color;
        }

        private static void ApplyButtonColors(UnityEngine.UI.Button button, StartMenuTheme theme)
        {
            if (button == null)
            {
                return;
            }

            UnityEngine.UI.ColorBlock colors = button.colors;
            colors.normalColor = theme.ButtonColor;
            colors.highlightedColor = theme.ButtonHoverColor;
            colors.selectedColor = Color.Lerp(theme.ButtonColor, theme.ButtonHoverColor, 0.65f);
            colors.pressedColor = Color.Lerp(theme.ButtonHoverColor, Color.black, 0.2f);
            colors.disabledColor = new Color(theme.ButtonColor.r, theme.ButtonColor.g, theme.ButtonColor.b, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
        }
    }
}
