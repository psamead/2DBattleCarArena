using TMPro;
using UnityEngine;

namespace BattleCarArena.UI
{
    public sealed class StartMenuView : MonoBehaviour
    {
        [Header("Replaceable Visual Slots")]
        [SerializeField] private UnityEngine.UI.Image backgroundImage;
        [SerializeField] private UnityEngine.UI.Image carForegroundImage;
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
        private AudioClip optionHoverSound;
        private AudioClip optionConfirmSound;

        public void Configure(
            UnityEngine.UI.Image background,
            UnityEngine.UI.Image carForeground,
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
            carForegroundImage = carForeground;
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

            optionHoverSound = theme.OptionHoverSound;
            optionConfirmSound = theme.OptionConfirmSound;

            // Keep the replaceable background artwork visible at full color. The
            // theme background color remains the fallback when no sprite is set.
            Color backgroundColor = theme.BackgroundSprite != null ? Color.white : theme.BackgroundColor;
            ApplyImage(backgroundImage, theme.BackgroundSprite, backgroundColor);
            ApplyImage(carForegroundImage, theme.CarForegroundSprite, Color.white);
            if (carForegroundImage != null)
            {
                carForegroundImage.preserveAspect = true;
                carForegroundImage.raycastTarget = false;
                carForegroundImage.gameObject.SetActive(theme.CarForegroundSprite != null);
            }
            ApplyImage(panelImage, null, theme.PanelColor);
            ApplyImage(accentImage, null, theme.AccentColor);
            Color buttonColor = theme.ButtonSprite != null ? Color.white : theme.ButtonColor;
            ApplyImage(startButtonImage, theme.ButtonSprite, buttonColor);
            ApplyImage(exitButtonImage, theme.ButtonSprite, buttonColor);

            if (logoImage != null)
            {
                logoImage.sprite = theme.LogoSprite;
                logoImage.color = Color.white;
                logoImage.preserveAspect = true;
                logoImage.gameObject.SetActive(theme.LogoSprite != null);
            }

            if (titleText != null)
            {
                titleText.gameObject.SetActive(theme.LogoSprite == null);
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

        public void PlayOptionHoverSound()
        {
            PlayUiSound(optionHoverSound);
        }

        public void PlayOptionConfirmSound()
        {
            if (optionConfirmSound != null)
            {
                BattleCarArena.Core.GameSession.Instance.PlayUiSound(optionConfirmSound);
            }
        }

        public void EnsureOptionHoverFeedback()
        {
            ConfigureHoverFeedback(startButton);
            ConfigureHoverFeedback(exitButton);
        }

        private void PlayUiSound(AudioClip clip)
        {
            if (musicSource != null && clip != null)
            {
                musicSource.PlayOneShot(clip);
            }
        }

        private void ConfigureHoverFeedback(UnityEngine.UI.Button button)
        {
            if (button == null)
            {
                return;
            }

            StartMenuHoverFeedback feedback = button.GetComponent<StartMenuHoverFeedback>();
            if (feedback == null)
            {
                feedback = button.gameObject.AddComponent<StartMenuHoverFeedback>();
            }

            feedback.Configure(this);
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
            Color normalColor = theme.ButtonSprite != null ? Color.white : theme.ButtonColor;
            colors.normalColor = normalColor;
            colors.highlightedColor = theme.ButtonHoverColor;
            colors.selectedColor = Color.Lerp(normalColor, theme.ButtonHoverColor, 0.65f);
            colors.pressedColor = Color.Lerp(theme.ButtonHoverColor, Color.black, 0.2f);
            colors.disabledColor = new Color(normalColor.r, normalColor.g, normalColor.b, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
        }
    }
}
