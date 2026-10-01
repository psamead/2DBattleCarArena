using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleCarArena.Battle
{
    public sealed class BattleResultView : MonoBehaviour
    {
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private TMP_Text reasonText;

        public void Show(BattleResolution resolution)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (resultText != null)
            {
                resultText.text = resolution.Winner == BattleSide.Player ? "PLAYER WINS" : "CHALLENGER WINS";
            }

            if (reasonText != null)
            {
                reasonText.text = resolution.Reason switch
                {
                    BattleEndReason.BoundaryHit => "PUSHED INTO THE WALL",
                    BattleEndReason.TimeLimit => "TIME LIMIT - HEALTH LEAD",
                    _ => "HEALTH DEPLETED"
                };
            }
        }

        public void Hide()
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
        }
    }
}
