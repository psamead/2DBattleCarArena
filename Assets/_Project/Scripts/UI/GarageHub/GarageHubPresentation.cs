using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace BattleCarArena.UI
{
    public sealed class GarageHubPresentation : MonoBehaviour
    {
        [Header("Theme")]
        [SerializeField] private GarageHubTheme theme;

        [Header("Scene Slots")]
        [SerializeField] private Image neutralBackgroundImage;
        [SerializeField] private Image alternateBackgroundImage;
        [SerializeField] private Image flickerOverlayImage;
        [SerializeField] private Image titleLogoImage;
        [SerializeField] private Image carPreviewImage;
        [SerializeField] private Image[] upgradeIconImages;
        [SerializeField] private Image[] panelImages;
        [SerializeField] private Image[] buttonImages;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text carPreviewPlaceholder;
        [SerializeField] private TMP_Text[] upgradeIconPlaceholders;
        [SerializeField] private AudioSource musicSource;

        private TMP_Text[] textElements;
        private System.Random flickerRandom;
        private float breathingTime;
        private float flickerCooldown;
        private float flickerTimeRemaining;
        private float activeFlickerStrength;

        private void OnEnable()
        {
            textElements = GetComponentsInChildren<TMP_Text>(true);
            InitializeFlickerRandom();
            ApplyTheme();
            ScheduleNextFlicker();
        }

        private void OnDisable()
        {
            if (flickerOverlayImage != null)
            {
                SetAlpha(flickerOverlayImage, 0f);
            }

            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        private void Update()
        {
            if (theme == null)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            float cycleDuration = Mathf.Max(1f, theme.BreathingCycleSeconds);
            breathingTime = (breathingTime + deltaTime / cycleDuration) % 1f;

            // Ease between the neutral and alternate sprites, then back again.
            float blend = 0.5f - 0.5f * Mathf.Cos(breathingTime * Mathf.PI * 2f);
            if (alternateBackgroundImage != null)
            {
                SetAlpha(alternateBackgroundImage, blend);
            }

            UpdateFlicker(deltaTime);
        }

        private void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }

            ApplySprite(neutralBackgroundImage, theme.NeutralBackground);
            ApplySprite(alternateBackgroundImage, theme.AlternateBackground);
            if (alternateBackgroundImage != null)
            {
                SetAlpha(alternateBackgroundImage, 0f);
                alternateBackgroundImage.gameObject.SetActive(theme.AlternateBackground != null);
            }

            if (flickerOverlayImage != null)
            {
                flickerOverlayImage.raycastTarget = false;
                SetAlpha(flickerOverlayImage, 0f);
            }

            ApplyOptionalSprite(titleLogoImage, theme.TitleLogo, true);
            if (titleLogoImage != null)
            {
                titleLogoImage.gameObject.SetActive(theme.TitleLogo != null);
            }

            if (titleText != null && titleLogoImage != null)
            {
                titleText.gameObject.SetActive(theme.TitleLogo == null);
            }

            ApplyOptionalSprite(carPreviewImage, theme.CarPreview, true);
            SetPlaceholderActive(carPreviewPlaceholder, theme.CarPreview == null);
            ApplyOptionalSprites(upgradeIconImages, theme.UpgradeIcon, true);
            SetPlaceholdersActive(upgradeIconPlaceholders, theme.UpgradeIcon == null);
            ApplyOptionalSprites(panelImages, theme.Panel, false);
            ApplyOptionalSprites(buttonImages, theme.Button, false);

            if (theme.TitleFont != null && textElements != null)
            {
                foreach (TMP_Text text in textElements)
                {
                    if (text != null)
                    {
                        text.font = theme.TitleFont;
                    }
                }
            }

            if (musicSource != null)
            {
                musicSource.Stop();
                musicSource.clip = theme.BackgroundMusic;
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.spatialBlend = 0f;
                musicSource.volume = theme.MusicVolume;

                if (musicSource.clip != null)
                {
                    musicSource.Play();
                }
            }
        }

        private void UpdateFlicker(float deltaTime)
        {
            if (flickerOverlayImage == null || theme.FlickerStrength <= 0f)
            {
                return;
            }

            if (flickerTimeRemaining > 0f)
            {
                flickerTimeRemaining -= deltaTime;
                float duration = Mathf.Max(0.02f, theme.FlickerDuration);
                float progress = 1f - Mathf.Clamp01(flickerTimeRemaining / duration);
                float envelope = Mathf.Sin(progress * Mathf.PI);
                float frequency = Mathf.Max(0.1f, theme.FlickerFrequencyHz);
                float flickerElapsed = duration - flickerTimeRemaining;
                float pulse = Mathf.Max(0f, Mathf.Sin(flickerElapsed * frequency * Mathf.PI * 2f));
                SetAlpha(flickerOverlayImage, activeFlickerStrength * envelope * pulse);

                if (flickerTimeRemaining <= 0f)
                {
                    SetAlpha(flickerOverlayImage, 0f);
                    ScheduleNextFlicker();
                }

                return;
            }

            flickerCooldown -= deltaTime;
            if (flickerCooldown <= 0f)
            {
                flickerTimeRemaining = Mathf.Max(0.02f, theme.FlickerDuration);
                activeFlickerStrength = NextRandomRange(theme.FlickerStrength * 0.45f, theme.FlickerStrength);
            }
        }

        private void InitializeFlickerRandom()
        {
            int seed = theme != null ? theme.RandomSeed : 0;
            if (seed == 0)
            {
                seed = unchecked((int)DateTime.UtcNow.Ticks) ^ GetInstanceID();
            }

            flickerRandom = new System.Random(seed);
        }

        private void ScheduleNextFlicker()
        {
            if (theme == null)
            {
                flickerCooldown = 0f;
                return;
            }

            float minimum = Mathf.Max(0f, theme.FlickerIntervalMinimum);
            float maximum = Mathf.Max(minimum, theme.FlickerIntervalMaximum);
            flickerCooldown = NextRandomRange(minimum, maximum);
        }

        private float NextRandomRange(float minimum, float maximum)
        {
            if (maximum <= minimum)
            {
                return minimum;
            }

            double sample = flickerRandom != null ? flickerRandom.NextDouble() : 0.5d;
            return minimum + (float)sample * (maximum - minimum);
        }

        private static void ApplySprite(Image image, Sprite sprite)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        private static void ApplyOptionalSprite(Image image, Sprite sprite, bool preserveAspect)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.raycastTarget = false;
            if (sprite != null)
            {
                image.color = Color.white;
                image.preserveAspect = preserveAspect;
            }
        }

        private static void ApplyOptionalSprites(Image[] images, Sprite sprite, bool preserveAspect)
        {
            if (images == null)
            {
                return;
            }

            foreach (Image image in images)
            {
                ApplyOptionalSprite(image, sprite, preserveAspect);
            }
        }

        private static void SetPlaceholderActive(TMP_Text placeholder, bool active)
        {
            if (placeholder != null)
            {
                placeholder.gameObject.SetActive(active);
            }
        }

        private static void SetPlaceholdersActive(TMP_Text[] placeholders, bool active)
        {
            if (placeholders == null)
            {
                return;
            }

            foreach (TMP_Text placeholder in placeholders)
            {
                SetPlaceholderActive(placeholder, active);
            }
        }

        private static void SetAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = Mathf.Clamp01(alpha);
            image.color = color;
        }
    }
}
