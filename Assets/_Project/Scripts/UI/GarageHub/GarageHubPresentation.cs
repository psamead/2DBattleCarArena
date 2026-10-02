using TMPro;
using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private Image alternateCarPreviewImage;
        [SerializeField] private Image carFlickerOverlayImage;
        [SerializeField] private Image alternateCarFlickerOverlayImage;
        [SerializeField] private Image[] upgradeIconImages;
        [SerializeField] private Image[] panelImages;
        [SerializeField] private Image[] buttonImages;
        [SerializeField] private TMP_Text carPreviewPlaceholder;
        [SerializeField] private TMP_Text[] upgradeIconPlaceholders;
        [SerializeField] private AudioSource musicSource;

        [Header("Neon UI Motion")]
        [SerializeField, Min(0.1f)] private float neonPulseCycleSeconds = 2.4f;
        [SerializeField, Range(0f, 0.05f)] private float neonGrowAmplitude = 0.035f;
        [SerializeField, Range(0f, 1f)] private float neonGlowMinimum = 0.65f;

        private TMP_Text[] textElements;
        private NeonPulseTarget[] neonPulseTargets;
        private System.Random flickerRandom;
        private float breathingTime;
        private float flickerCooldown;
        private float flickerTimeRemaining;
        private float activeFlickerStrength;
        private float carBackgroundBlend;
        private readonly List<Graphic> transitionGraphics = new();
        private readonly List<Color> transitionBaseColors = new();
        private readonly List<int> transitionPhases = new();
        private bool transitionRunning;
        private float transitCarFade = 1f;
        private float transitSceneFade;
        private bool transitCrossfadeStarted;
        private Image entranceDarknessOverlay;
        [SerializeField, Range(0f, 1f)] private float arrivalStartingDarkness = 0.2f;
        private float arrivalVisualAlpha = 1f;

        private void Start()
        {
            if (BattleCarArena.Core.GameSession.Instance.ConsumeGarageEntrancePresentationPending())
            {
                CacheTransitionGraphics();
                CreateEntranceDarknessOverlay();
                arrivalVisualAlpha = 0f;
                SetTransitionAlpha(0f);
                StartCoroutine(PlayEntranceAnimation());
            }
        }

        public void PlaySelectionConfirmSound()
        {
            if (theme != null && theme.SelectionConfirmSound != null)
            {
                BattleCarArena.Core.GameSession.Instance.PlayUiSound(theme.SelectionConfirmSound);
            }
        }

        public void SetTransitCarFade(float alpha)
        {
            transitCarFade = Mathf.Clamp01(alpha);
        }

        public void SetTransitSceneFade(float alpha)
        {
            transitSceneFade = Mathf.Clamp01(alpha);
            if (!transitCrossfadeStarted)
            {
                CacheTransitionGraphics();
                transitCrossfadeStarted = true;
            }

            SetTransitionAlpha(1f - transitSceneFade);
        }

        public void PlayExitAnimation(Action onComplete)
        {
            if (transitionRunning)
            {
                return;
            }

            CacheTransitionGraphics();
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button button in buttons)
            {
                button.interactable = false;
            }

            StartCoroutine(PlayExitAnimationRoutine(onComplete));
        }

        private IEnumerator PlayEntranceAnimation()
        {
            transitionRunning = true;
            float elapsed = 0f;
            const float duration = 1f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                SetEntranceProgress(t);
                arrivalVisualAlpha = t;
                SetEntranceDarkness(Mathf.Lerp(arrivalStartingDarkness, 0f, t));
                yield return null;
            }

            SetTransitionAlpha(1f);
            arrivalVisualAlpha = 1f;
            SetEntranceDarkness(0f);
            if (entranceDarknessOverlay != null)
            {
                Destroy(entranceDarknessOverlay.gameObject);
                entranceDarknessOverlay = null;
            }
            transitionRunning = false;
        }

        private IEnumerator PlayExitAnimationRoutine(Action onComplete)
        {
            transitionRunning = true;
            float elapsed = 0f;
            const float duration = 0.45f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                for (int i = 0; i < transitionGraphics.Count; i++)
                {
                    float phaseStart = transitionPhases[i] * 0.12f;
                    float local = Mathf.Clamp01((t - phaseStart) / Mathf.Max(0.01f, 1f - phaseStart));
                    SetGraphicAlpha(transitionGraphics[i], transitionBaseColors[i], 1f - local);
                }

                yield return null;
            }

            SetTransitionAlpha(0f);
            transitionRunning = false;
            onComplete?.Invoke();
        }

        private void CacheTransitionGraphics()
        {
            transitionGraphics.Clear();
            transitionBaseColors.Clear();
            transitionPhases.Clear();
            HashSet<Graphic> seen = new();
            AddTransitionGraphic(titleLogoImage, 0, seen);
            AddTransitionGraphics(panelImages, 1, seen);
            AddTransitionGraphics(upgradeIconImages, 1, seen);
            if (textElements != null)
            {
                foreach (TMP_Text text in textElements)
                {
                    AddTransitionGraphic(text, 1, seen);
                }
            }
            AddTransitionGraphics(buttonImages, 2, seen);
            ExcludeTransitionGraphicTree(carPreviewImage);
            ExcludeTransitionGraphicTree(alternateCarPreviewImage);
            ExcludeTransitionGraphicTree(carFlickerOverlayImage);
            ExcludeTransitionGraphicTree(alternateCarFlickerOverlayImage);
        }

        private void ExcludeTransitionGraphicTree(Graphic root)
        {
            if (root == null) return;
            Graphic[] graphics = root.GetComponentsInChildren<Graphic>(true);
            for (int graphicIndex = graphics.Length - 1; graphicIndex >= 0; graphicIndex--)
            {
                int transitionIndex = transitionGraphics.IndexOf(graphics[graphicIndex]);
                if (transitionIndex < 0) continue;
                transitionGraphics.RemoveAt(transitionIndex);
                transitionBaseColors.RemoveAt(transitionIndex);
                transitionPhases.RemoveAt(transitionIndex);
            }
        }

        private void CreateEntranceDarknessOverlay()
        {
            Canvas canvas = GetComponentInChildren<Canvas>(true);
            if (canvas == null) return;

            GameObject overlayObject = new("GarageArrivalDarkness", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(canvas.transform, false);
            RectTransform rect = (RectTransform)overlayObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            entranceDarknessOverlay = overlayObject.GetComponent<Image>();
            entranceDarknessOverlay.color = new Color(0f, 0f, 0f, arrivalStartingDarkness);
            entranceDarknessOverlay.raycastTarget = false;
            rect.SetAsLastSibling();
        }

        private void SetEntranceDarkness(float alpha)
        {
            if (entranceDarknessOverlay == null) return;
            Color color = entranceDarknessOverlay.color;
            color.a = alpha;
            entranceDarknessOverlay.color = color;
        }

        private void AddTransitionGraphics(IEnumerable<Image> images, int phase, HashSet<Graphic> seen)
        {
            if (images == null) return;
            foreach (Image image in images) AddTransitionGraphic(image, phase, seen);
        }

        private void AddTransitionGraphic(Graphic graphic, int phase, HashSet<Graphic> seen)
        {
            if (graphic == null || !seen.Add(graphic)) return;
            transitionGraphics.Add(graphic);
            transitionBaseColors.Add(graphic.color);
            transitionPhases.Add(phase);

            foreach (Graphic childGraphic in graphic.GetComponentsInChildren<Graphic>(true))
            {
                if (childGraphic == graphic || !seen.Add(childGraphic)) continue;
                transitionGraphics.Add(childGraphic);
                transitionBaseColors.Add(childGraphic.color);
                transitionPhases.Add(phase);
            }
        }

        private void SetEntranceProgress(float progress)
        {
            for (int i = 0; i < transitionGraphics.Count; i++)
            {
                float phaseStart = transitionPhases[i] * 0.18f;
                float local = Mathf.Clamp01((progress - phaseStart) / Mathf.Max(0.01f, 1f - phaseStart));
                SetGraphicAlpha(transitionGraphics[i], transitionBaseColors[i], local);
            }
        }

        private void SetTransitionAlpha(float alpha)
        {
            for (int i = 0; i < transitionGraphics.Count; i++)
            {
                SetGraphicAlpha(transitionGraphics[i], transitionBaseColors[i], alpha);
            }
        }

        private static void SetGraphicAlpha(Graphic graphic, Color baseColor, float alpha)
        {
            if (graphic == null) return;
            Color color = baseColor;
            color.a *= alpha;
            graphic.color = color;
        }

        private void OnEnable()
        {
            textElements = GetComponentsInChildren<TMP_Text>(true);
            CacheNeonPulseTargets();
            InitializeFlickerRandom();
            ApplyTheme();
            ScheduleNextFlicker();
        }

        private void OnDisable()
        {
            ApplyCarFlicker(0f);
            ResetNeonPulseTargets();

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
            if (!transitionRunning)
            {
                UpdateNeonPulseTargets();
            }

            if (theme == null)
            {
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            float cycleDuration = Mathf.Max(1f, theme.BreathingCycleSeconds);
            breathingTime = (breathingTime + deltaTime / cycleDuration) % 1f;

            // Ease between the neutral and alternate sprites, then back again.
            float blend = 0.5f - 0.5f * Mathf.Cos(breathingTime * Mathf.PI * 2f);
            carBackgroundBlend = blend;
            if (neutralBackgroundImage != null)
            {
                SetAlpha(neutralBackgroundImage, arrivalVisualAlpha * (1f - transitSceneFade));
            }
            if (carPreviewImage != null)
            {
                Color neutralColor = carPreviewImage.color;
                neutralColor.a = Mathf.Lerp(1f, 0.9f, blend) * arrivalVisualAlpha * transitCarFade;
                carPreviewImage.color = neutralColor;
            }
            if (alternateBackgroundImage != null)
            {
                SetAlpha(alternateBackgroundImage, blend * arrivalVisualAlpha * (1f - transitSceneFade));
            }
            if (alternateCarPreviewImage != null)
            {
                SetAlpha(alternateCarPreviewImage, blend * theme.AlternateCarPreviewTint.a * arrivalVisualAlpha * transitCarFade);
            }

            UpdateFlicker(deltaTime);
        }

        private void CacheNeonPulseTargets()
        {
            RectTransform[] rectTransforms = GetComponentsInChildren<RectTransform>(true);
            System.Collections.Generic.List<NeonPulseTarget> targets = new();

            foreach (RectTransform rectTransform in rectTransforms)
            {
                if (rectTransform.name != "NeonFrame")
                {
                    continue;
                }

                System.Collections.Generic.List<Image> glowImageList = new();
                Transform pinkGlowTransform = rectTransform.Find("PinkGlow");
                Transform blueGlowTransform = rectTransform.Find("BlueGlow");
                if (pinkGlowTransform != null)
                {
                    glowImageList.AddRange(pinkGlowTransform.GetComponentsInChildren<Image>(true));
                }
                if (blueGlowTransform != null)
                {
                    glowImageList.AddRange(blueGlowTransform.GetComponentsInChildren<Image>(true));
                }

                Image[] glowImages = glowImageList.ToArray();
                Color[] originalColors = new Color[glowImages.Length];
                for (int i = 0; i < glowImages.Length; i++)
                {
                    originalColors[i] = glowImages[i].color;
                }

                targets.Add(new NeonPulseTarget(rectTransform, rectTransform.localScale, glowImages, originalColors));
            }

            neonPulseTargets = targets.ToArray();
        }

        private void UpdateNeonPulseTargets()
        {
            if (neonPulseTargets == null || neonPulseTargets.Length == 0)
            {
                return;
            }

            float cycleSeconds = Mathf.Max(0.1f, neonPulseCycleSeconds);
            float cycle = Time.unscaledTime * (Mathf.PI * 2f / cycleSeconds);
            for (int i = 0; i < neonPulseTargets.Length; i++)
            {
                NeonPulseTarget target = neonPulseTargets[i];
                if (target.Frame == null)
                {
                    continue;
                }

                float pulse = 0.5f - 0.5f * Mathf.Cos(cycle + i * 0.37f);
                target.Frame.localScale = target.BaseScale * (1f + neonGrowAmplitude * pulse);
                float glow = Mathf.Lerp(neonGlowMinimum, 1f, pulse);
                for (int imageIndex = 0; imageIndex < target.GlowImages.Length; imageIndex++)
                {
                    Image image = target.GlowImages[imageIndex];
                    if (image == null)
                    {
                        continue;
                    }

                    Color color = target.OriginalColors[imageIndex];
                    color.a *= glow;
                    image.color = color;
                }
            }
        }

        private void ResetNeonPulseTargets()
        {
            if (neonPulseTargets == null)
            {
                return;
            }

            foreach (NeonPulseTarget target in neonPulseTargets)
            {
                if (target.Frame != null)
                {
                    target.Frame.localScale = target.BaseScale;
                }

                for (int i = 0; i < target.GlowImages.Length; i++)
                {
                    if (target.GlowImages[i] != null)
                    {
                        target.GlowImages[i].color = target.OriginalColors[i];
                    }
                }
            }
        }

        private readonly struct NeonPulseTarget
        {
            public readonly RectTransform Frame;
            public readonly Vector3 BaseScale;
            public readonly Image[] GlowImages;
            public readonly Color[] OriginalColors;

            public NeonPulseTarget(RectTransform frame, Vector3 baseScale, Image[] glowImages, Color[] originalColors)
            {
                Frame = frame;
                BaseScale = baseScale;
                GlowImages = glowImages;
                OriginalColors = originalColors;
            }
        }

        private void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }

            EnsureAlternateCarPreview();
            EnsureCarFlickerOverlay();
            EnsureAlternateCarFlickerOverlay();
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

            // The logo is a scene-authored image so its manually adjusted RectTransform stays authoritative.
            // Update only its sprite; never activate another logo slot or toggle a second title object.
            if (titleLogoImage != null && theme.TitleLogo != null)
            {
                ApplyOptionalSprite(titleLogoImage, theme.TitleLogo, true);
            }

            ApplyOptionalSprite(carPreviewImage, theme.CarPreview, true);
            if (carPreviewImage != null && theme.CarPreview != null)
            {
                Color neutralTint = theme.CarPreviewTint;
                neutralTint.a = 1f;
                carPreviewImage.color = neutralTint;
            }
            ApplyOptionalSprite(alternateCarPreviewImage, theme.AlternateCarPreview, true);
            if (alternateCarPreviewImage != null)
            {
                alternateCarPreviewImage.color = theme.AlternateCarPreviewTint;
                alternateCarPreviewImage.raycastTarget = false;
                SetAlpha(alternateCarPreviewImage, 0f);
                alternateCarPreviewImage.gameObject.SetActive(theme.AlternateCarPreview != null);
            }
            ApplyOptionalSprite(carFlickerOverlayImage, theme.CarPreview, true);
            if (carFlickerOverlayImage != null)
            {
                carFlickerOverlayImage.raycastTarget = false;
                SetAlpha(carFlickerOverlayImage, 0f);
            }
            ApplyOptionalSprite(alternateCarFlickerOverlayImage, theme.AlternateCarPreview, false);
            if (alternateCarFlickerOverlayImage != null)
            {
                alternateCarFlickerOverlayImage.raycastTarget = false;
                alternateCarFlickerOverlayImage.preserveAspect = false;
                SetAlpha(alternateCarFlickerOverlayImage, 0f);
                alternateCarFlickerOverlayImage.gameObject.SetActive(theme.AlternateCarPreview != null);
            }
            SetPlaceholderActive(carPreviewPlaceholder, theme.CarPreview == null);
            ApplyUpgradeIcons();
            ApplyOptionalSprites(panelImages, theme.Panel, false);
            ApplyOptionalSprites(buttonImages, theme.Button, false);
            RestoreButtonRaycastTargets();
            ApplySlotFrames();

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

            ApplyActionButtonTheme();

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

        private void ApplySlotFrames()
        {
            if (theme == null || theme.SlotFrame == null || panelImages == null)
            {
                return;
            }

            foreach (Image image in panelImages)
            {
                if (image == null || !UsesDedicatedSlotFrame(image.gameObject.name))
                {
                    continue;
                }

                image.sprite = theme.SlotFrame;
                image.type = Image.Type.Sliced;
                image.preserveAspect = false;
                bool isUpgradeCard = image.gameObject.name == "ENGINEUpgradeCard" || image.gameObject.name == "WEAPONUpgradeCard" || image.gameObject.name == "ARMORUpgradeCard";
                image.color = isUpgradeCard ? new Color(0.075f, 0.095f, 0.13f, 0.25f) : Color.white;
                image.raycastTarget = false;
            }
        }

        private void ApplyUpgradeIcons()
        {
            Sprite[] icons =
            {
                theme.EngineUpgradeIcon,
                theme.WeaponUpgradeIcon,
                theme.ArmorUpgradeIcon
            };

            int imageCount = upgradeIconImages != null ? upgradeIconImages.Length : 0;
            int placeholderCount = upgradeIconPlaceholders != null ? upgradeIconPlaceholders.Length : 0;
            int slotCount = Mathf.Max(imageCount, placeholderCount);
            for (int i = 0; i < slotCount; i++)
            {
                Sprite icon = i < icons.Length ? icons[i] : null;
                if (i < imageCount)
                {
                    ApplyOptionalSprite(upgradeIconImages[i], icon, true);
                }

                if (i < placeholderCount)
                {
                    SetPlaceholderActive(upgradeIconPlaceholders[i], icon == null);
                }
            }
        }

        private void ApplyActionButtonTheme()
        {
            if (buttonImages == null)
            {
                return;
            }

            foreach (Image image in buttonImages)
            {
                if (image == null
                    || (image.gameObject.name != "GoToMissionButton" && image.gameObject.name != "BackToMenuButton"))
                {
                    continue;
                }

                Sprite frame = image.gameObject.name == "GoToMissionButton"
                    ? (theme.GoToMissionButtonFrame != null ? theme.GoToMissionButtonFrame : theme.ActionButtonFrame)
                    : theme.ActionButtonFrame;
                if (frame != null)
                {
                    image.sprite = frame;
                    image.type = Image.Type.Simple;
                    image.preserveAspect = false;
                    image.color = Color.white;
                    image.raycastTarget = true;
                }

                if (theme.ActionButtonFont != null)
                {
                    TMP_Text label = image.GetComponentInChildren<TMP_Text>(true);
                    if (label != null)
                    {
                        label.font = theme.ActionButtonFont;
                    }
                }
            }
        }

        private void RestoreButtonRaycastTargets()
        {
            if (buttonImages == null)
            {
                return;
            }

            foreach (Image image in buttonImages)
            {
                if (image == null)
                {
                    continue;
                }

                Button button = image.GetComponent<Button>();
                if (button != null && button.targetGraphic == image)
                {
                    image.raycastTarget = true;
                }
            }
        }

        private static bool UsesDedicatedSlotFrame(string objectName)
        {
            return objectName == "ENGINEUpgradeCard"
                || objectName == "WEAPONUpgradeCard"
                || objectName == "ARMORUpgradeCard"
                || objectName == "EnergyPanel"
                || objectName == "CreditsStat"
                || objectName == "ScoreStat"
                || objectName == "RankStat";
        }

        private void EnsureCarFlickerOverlay()
        {
            if (carFlickerOverlayImage != null || carPreviewImage == null || !Application.isPlaying)
            {
                return;
            }

            Transform sourceTransform = carPreviewImage.transform;
            Transform parent = sourceTransform.parent;
            if (parent == null)
            {
                return;
            }

            GameObject overlayObject = new("CarFlickerOverlay", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(parent, false);

            RectTransform sourceRect = carPreviewImage.rectTransform;
            RectTransform overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = sourceRect.anchorMin;
            overlayRect.anchorMax = sourceRect.anchorMax;
            overlayRect.pivot = sourceRect.pivot;
            overlayRect.anchoredPosition = sourceRect.anchoredPosition;
            overlayRect.sizeDelta = sourceRect.sizeDelta;
            overlayRect.localRotation = sourceRect.localRotation;
            overlayRect.localScale = sourceRect.localScale;
            int overlayOrder = sourceTransform.GetSiblingIndex() + (alternateCarPreviewImage != null ? 2 : 1);
            overlayRect.SetSiblingIndex(overlayOrder);

            carFlickerOverlayImage = overlayObject.GetComponent<Image>();
            carFlickerOverlayImage.raycastTarget = false;
            carFlickerOverlayImage.maskable = true;
        }

        private void EnsureAlternateCarPreview()
        {
            if (alternateCarPreviewImage != null || carPreviewImage == null || !Application.isPlaying)
            {
                return;
            }

            Transform sourceTransform = carPreviewImage.transform;
            Transform parent = sourceTransform.parent;
            if (parent == null)
            {
                return;
            }

            GameObject overlayObject = new("AlternateCarPreview", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(parent, false);

            RectTransform sourceRect = carPreviewImage.rectTransform;
            RectTransform overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = sourceRect.anchorMin;
            overlayRect.anchorMax = sourceRect.anchorMax;
            overlayRect.pivot = sourceRect.pivot;
            overlayRect.anchoredPosition = sourceRect.anchoredPosition;
            overlayRect.sizeDelta = sourceRect.sizeDelta;
            overlayRect.localRotation = sourceRect.localRotation;
            overlayRect.localScale = sourceRect.localScale;
            overlayRect.SetSiblingIndex(sourceTransform.GetSiblingIndex() + 1);

            alternateCarPreviewImage = overlayObject.GetComponent<Image>();
            alternateCarPreviewImage.raycastTarget = false;
            alternateCarPreviewImage.maskable = true;
        }

        private void EnsureAlternateCarFlickerOverlay()
        {
            if (alternateCarFlickerOverlayImage != null || alternateCarPreviewImage == null || !Application.isPlaying)
            {
                return;
            }

            Transform sourceTransform = alternateCarPreviewImage.transform;
            Transform parent = sourceTransform.parent;
            if (parent == null)
            {
                return;
            }

            GameObject overlayObject = new("AlternateCarFlickerOverlay", typeof(RectTransform), typeof(Image));
            overlayObject.transform.SetParent(parent, false);

            RectTransform sourceRect = alternateCarPreviewImage.rectTransform;
            RectTransform overlayRect = (RectTransform)overlayObject.transform;
            overlayRect.anchorMin = sourceRect.anchorMin;
            overlayRect.anchorMax = sourceRect.anchorMax;
            overlayRect.pivot = sourceRect.pivot;
            overlayRect.anchoredPosition = sourceRect.anchoredPosition;
            overlayRect.sizeDelta = sourceRect.sizeDelta;
            overlayRect.localRotation = sourceRect.localRotation;
            overlayRect.localScale = sourceRect.localScale;
            overlayRect.SetSiblingIndex(sourceTransform.GetSiblingIndex() + 2);

            alternateCarFlickerOverlayImage = overlayObject.GetComponent<Image>();
            alternateCarFlickerOverlayImage.raycastTarget = false;
            alternateCarFlickerOverlayImage.maskable = true;
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
                SetAlpha(flickerOverlayImage, activeFlickerStrength * envelope * pulse * arrivalVisualAlpha);
                float flickerScale = theme.FlickerStrength > 0f
                    ? activeFlickerStrength / theme.FlickerStrength
                    : 0f;
                ApplyCarFlicker(envelope * pulse * flickerScale);

                if (flickerTimeRemaining <= 0f)
                {
                    SetAlpha(flickerOverlayImage, 0f);
                    ApplyCarFlicker(0f);
                    ScheduleNextFlicker();
                }

                return;
            }

            flickerCooldown -= deltaTime;
            ApplyCarFlicker(0f);
            if (flickerCooldown <= 0f)
            {
                flickerTimeRemaining = Mathf.Max(0.02f, theme.FlickerDuration);
                activeFlickerStrength = NextRandomRange(theme.FlickerStrength * 0.45f, theme.FlickerStrength);
            }
        }

        private void ApplyCarFlicker(float pulse)
        {
            if (carFlickerOverlayImage == null || theme == null)
            {
                return;
            }

            carFlickerOverlayImage.color = theme.CarFlickerTint;
            float intensity = Mathf.Clamp01(pulse * theme.CarFlickerResponse) * arrivalVisualAlpha * transitCarFade;
            SetAlpha(carFlickerOverlayImage, intensity * (1f - carBackgroundBlend));
            if (alternateCarFlickerOverlayImage != null)
            {
                alternateCarFlickerOverlayImage.color = theme.CarFlickerTint;
                SetAlpha(alternateCarFlickerOverlayImage, intensity * carBackgroundBlend);
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
