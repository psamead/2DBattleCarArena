using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleCarArena.Battle
{
    public sealed class BattleHudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text playerNameText;
        [SerializeField] private TMP_Text challengerNameText;
        [SerializeField] private TMP_Text playerHealthText;
        [SerializeField] private TMP_Text challengerHealthText;
        [SerializeField] private Image playerHealthFill;
        [SerializeField] private Image challengerHealthFill;
        [SerializeField] private Image playerHealthFrame;
        [SerializeField] private Image challengerHealthFrame;
        [SerializeField] private TMP_Text startCueText;
        [SerializeField] private BattleResultView resultView;
        [Header("Impact Shake")]
        [SerializeField, Min(0f)] private float impactShakeDistance = 12f;
        [SerializeField, Min(0.01f)] private float impactShakeDuration = 0.28f;
        [SerializeField, Min(0f)] private float impactShakeFrequency = 28f;
        [SerializeField, Range(0f, 0.5f)] private float motionTrailOpacity = 0.2f;
        [SerializeField, Min(0f)] private float motionTrailDistance = 8f;

        private sealed class MotionTrail
        {
            public Graphic Source;
            public Graphic Ghost;
        }

        private RectTransform shakeRoot;
        private readonly List<MotionTrail> motionTrails = new();
        private Vector2 restPosition;
        private float shakeRemaining;
        private float shakeStrength;
        private float shakePhase;

        public BattleResultView ResultView => resultView;

        private void Awake()
        {
            shakeRoot = transform as RectTransform;
            if (shakeRoot == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                if (canvas != null) shakeRoot = canvas.transform as RectTransform;
            }
            if (shakeRoot != null)
                restPosition = shakeRoot.anchoredPosition;

            CreateMotionTrail(playerNameText);
            CreateMotionTrail(challengerNameText);
            CreateMotionTrail(playerHealthText);
            CreateMotionTrail(challengerHealthText);
            CreateMotionTrail(playerHealthFill);
            CreateMotionTrail(challengerHealthFill);
            CreateMotionTrail(playerHealthFrame);
            CreateMotionTrail(challengerHealthFrame);
            CreateMotionTrail(startCueText);
        }

        private void LateUpdate()
        {
            if (shakeRoot == null) return;
            shakeRemaining = Mathf.Max(0f, shakeRemaining - Time.unscaledDeltaTime);
            if (shakeRemaining <= 0f || shakeStrength <= 0f)
            {
                shakeRoot.anchoredPosition = restPosition;
                shakeStrength = 0f;
                SetMotionTrails(Vector2.zero, 0f);
                return;
            }

            shakePhase += Time.unscaledDeltaTime * impactShakeFrequency;
            float fade = shakeRemaining / impactShakeDuration;
            Vector2 direction = new Vector2(Mathf.Sin(shakePhase * 1.7f), Mathf.Cos(shakePhase * 2.3f)).normalized;
            Vector2 offset = direction * (impactShakeDistance * shakeStrength * fade);
            shakeRoot.anchoredPosition = restPosition + offset;
            SetMotionTrails(offset, fade);
        }

        public void TriggerImpact(float intensity = 1f)
        {
            shakeRemaining = Mathf.Max(shakeRemaining, impactShakeDuration);
            shakeStrength = Mathf.Max(shakeStrength, Mathf.Clamp(intensity, 0f, 1.15f));
            if (shakeRoot != null)
            {
                Vector2 offset = Vector2.right * (impactShakeDistance * shakeStrength * 0.25f);
                shakeRoot.anchoredPosition = restPosition + offset;
                SetMotionTrails(offset, 1f);
            }
        }

        private void CreateMotionTrail(Graphic source)
        {
            if (source == null) return;
            GameObject ghostObject = Instantiate(source.gameObject, source.transform.parent);
            ghostObject.name = source.gameObject.name + " ImpactTrail";
            Graphic ghost = ghostObject.GetComponent<Graphic>();
            if (ghost == null)
            {
                Destroy(ghostObject);
                return;
            }

            ghost.raycastTarget = false;
            ghostObject.transform.SetAsFirstSibling();
            ghostObject.SetActive(false);
            motionTrails.Add(new MotionTrail { Source = source, Ghost = ghost });
        }

        private void SetMotionTrails(Vector2 shakeOffset, float fade)
        {
            bool visible = fade > 0f && motionTrailOpacity > 0f;
            foreach (MotionTrail trail in motionTrails)
            {
                if (trail.Source == null || trail.Ghost == null) continue;
                if (trail.Ghost.gameObject.activeSelf != visible)
                    trail.Ghost.gameObject.SetActive(visible);
                if (!visible) continue;

                if (trail.Source is TMP_Text sourceText && trail.Ghost is TMP_Text ghostText)
                    ghostText.text = sourceText.text;
                if (trail.Source is Image sourceImage && trail.Ghost is Image ghostImage)
                    ghostImage.fillAmount = sourceImage.fillAmount;

                Color ghostColor = trail.Source.color;
                ghostColor.a *= motionTrailOpacity * fade * shakeStrength;
                trail.Ghost.color = ghostColor;
                RectTransform sourceRect = trail.Source.transform as RectTransform;
                RectTransform ghostRect = trail.Ghost.transform as RectTransform;
                if (sourceRect != null && ghostRect != null)
                    ghostRect.anchoredPosition = sourceRect.anchoredPosition - shakeOffset.normalized * (motionTrailDistance * fade);
            }
        }

        public void SetNames(string playerName, string challengerName)
        {
            if (playerNameText != null) playerNameText.text = playerName;
            if (challengerNameText != null) challengerNameText.text = challengerName;
        }

        public void SetHealth(int playerHealth, int playerMaximum, int challengerHealth, int challengerMaximum)
        {
            SetHealthBar(playerHealthFill, playerHealthText, playerHealth, playerMaximum);
            SetHealthBar(challengerHealthFill, challengerHealthText, challengerHealth, challengerMaximum);
        }

        public void SetCue(string message)
        {
            if (startCueText != null)
            {
                startCueText.text = message;
            }
        }

        private static void SetHealthBar(Image fill, TMP_Text label, int health, int maximum)
        {
            int safeMaximum = Mathf.Max(1, maximum);
            int safeHealth = Mathf.Clamp(health, 0, safeMaximum);
            if (fill != null)
            {
                fill.fillAmount = safeHealth / (float)safeMaximum;
            }

            if (label != null)
            {
                label.text = $"{safeHealth} / {safeMaximum}";
            }
        }
    }
}
