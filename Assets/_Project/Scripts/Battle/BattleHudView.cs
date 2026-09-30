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

        public BattleResultView ResultView => resultView;

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
