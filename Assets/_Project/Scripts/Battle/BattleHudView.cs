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
        [SerializeField] private TMP_Text startCueText;
        [SerializeField] private BattleResultView resultView;
        [Header("Impact Shake")]
        [SerializeField, Min(0f)] private float impactShakeDistance = 5f;
        [SerializeField, Min(0.01f)] private float impactShakeDuration = 0.2f;
        [SerializeField, Min(0f)] private float impactShakeFrequency = 30f;

        private RectTransform shakeRoot;
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
        }

        private void LateUpdate()
        {
            if (shakeRoot == null) return;
            shakeRemaining = Mathf.Max(0f, shakeRemaining - Time.unscaledDeltaTime);
            if (shakeRemaining <= 0f || shakeStrength <= 0f)
            {
                shakeRoot.anchoredPosition = restPosition;
                shakeStrength = 0f;
                return;
            }

            shakePhase += Time.unscaledDeltaTime * impactShakeFrequency;
            float fade = shakeRemaining / impactShakeDuration;
            Vector2 direction = new(Mathf.Sin(shakePhase * 1.7f), Mathf.Cos(shakePhase * 2.3f));
            shakeRoot.anchoredPosition = restPosition + direction * (impactShakeDistance * shakeStrength * fade);
        }

        public void TriggerImpact(float intensity = 1f)
        {
            shakeRemaining = Mathf.Max(shakeRemaining, impactShakeDuration);
            shakeStrength = Mathf.Max(shakeStrength, Mathf.Clamp(intensity, 0f, 1.15f));
            if (shakeRoot != null)
                shakeRoot.anchoredPosition = restPosition + Vector2.right * (impactShakeDistance * shakeStrength * 0.25f);
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
