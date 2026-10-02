using System;
using BattleCarArena.Core;
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
        [SerializeField] private Button confirmButton;
        [SerializeField] private AudioClip confirmSound;

        public event Action Confirmed;

        private void Awake()
        {
            if (confirmButton == null && resultPanel != null)
            {
                confirmButton = resultPanel.GetComponent<Button>();
            }

            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(HandleConfirmClicked);
                confirmButton.interactable = false;
            }
        }

        private void OnDestroy()
        {
            if (confirmButton != null)
            {
                confirmButton.onClick.RemoveListener(HandleConfirmClicked);
            }
        }

        public void Show(BattleResolution resolution)
        {
            if (resultPanel != null)
            {
                resultPanel.SetActive(true);
            }

            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }

            if (resultText != null)
            {
                resultText.text = resolution.Winner == BattleSide.Player ? "PLAYER WINS" : "CHALLENGER WINS";
            }

            if (reasonText != null)
            {
                reasonText.text = string.Empty;
                reasonText.gameObject.SetActive(false);
            }
        }

        public void Hide()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }

            if (resultPanel != null)
            {
                resultPanel.SetActive(false);
            }
        }

        private void HandleConfirmClicked()
        {
            if (resultPanel != null && resultPanel.activeInHierarchy)
            {
                GameSession.Instance.PlayUiSound(confirmSound);
                Confirmed?.Invoke();
            }
        }
    }
}
