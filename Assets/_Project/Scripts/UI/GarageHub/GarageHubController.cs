using BattleCarArena.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BattleCarArena.UI
{
    public sealed class GarageHubController : MonoBehaviour
    {
        [Header("Upgrade Buttons")]
        [SerializeField] private Button engineUpgradeButton;
        [SerializeField] private Button weaponUpgradeButton;
        [SerializeField] private Button armorUpgradeButton;

        [Header("Navigation Buttons")]
        [SerializeField] private Button goToMissionButton;
        [SerializeField] private Button backToMenuButton;
        [SerializeField] private string battleSceneName = "BattleArena";
        [SerializeField] private string startMenuSceneName = "StartMenu";

        [Header("Displayed Values")]
        [SerializeField] private TMP_Text engineValueText;
        [SerializeField] private TMP_Text weaponValueText;
        [SerializeField] private TMP_Text armorValueText;
        [SerializeField] private TMP_Text creditsValueText;
        [SerializeField] private TMP_Text scoreValueText;
        [SerializeField] private TMP_Text energyValueText;

        private bool isLeavingGarage;

        private void OnEnable()
        {
            AddListeners();
            RefreshDisplay();
        }

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void AddListeners()
        {
            if (engineUpgradeButton != null) engineUpgradeButton.onClick.AddListener(PurchaseEngineUpgrade);
            if (weaponUpgradeButton != null) weaponUpgradeButton.onClick.AddListener(PurchaseWeaponUpgrade);
            if (armorUpgradeButton != null) armorUpgradeButton.onClick.AddListener(PurchaseArmorUpgrade);
            if (goToMissionButton != null) goToMissionButton.onClick.AddListener(GoToMission);
            if (backToMenuButton != null) backToMenuButton.onClick.AddListener(BackToMenu);
        }

        private void RemoveListeners()
        {
            if (engineUpgradeButton != null) engineUpgradeButton.onClick.RemoveListener(PurchaseEngineUpgrade);
            if (weaponUpgradeButton != null) weaponUpgradeButton.onClick.RemoveListener(PurchaseWeaponUpgrade);
            if (armorUpgradeButton != null) armorUpgradeButton.onClick.RemoveListener(PurchaseArmorUpgrade);
            if (goToMissionButton != null) goToMissionButton.onClick.RemoveListener(GoToMission);
            if (backToMenuButton != null) backToMenuButton.onClick.RemoveListener(BackToMenu);
        }

        private void PurchaseEngineUpgrade() => Purchase(GarageUpgradeType.Engine);
        private void PurchaseWeaponUpgrade() => Purchase(GarageUpgradeType.Weapon);
        private void PurchaseArmorUpgrade() => Purchase(GarageUpgradeType.Armor);

        private void Purchase(GarageUpgradeType upgradeType)
        {
            GarageProgress progress = GameSession.Instance.GarageProgress;
            if (progress.TryPurchase(upgradeType))
            {
                RefreshDisplay();
            }
        }

        private void RefreshDisplay()
        {
            GarageProgress progress = GameSession.Instance.GarageProgress;
            if (engineValueText != null) engineValueText.text = $"HPR: {progress.EnginePower}";
            if (weaponValueText != null) weaponValueText.text = $"DMG: {progress.WeaponDamage}";
            if (armorValueText != null) armorValueText.text = $"PWR: {progress.ArmorPower}";
            if (creditsValueText != null) creditsValueText.text = progress.Credits.ToString();
            if (scoreValueText != null) scoreValueText.text = progress.Score.ToString();
            if (energyValueText != null) energyValueText.text = $"{progress.EnergyPercent}%";

            if (engineUpgradeButton != null) engineUpgradeButton.interactable = progress.CanPurchase(GarageUpgradeType.Engine);
            if (weaponUpgradeButton != null) weaponUpgradeButton.interactable = progress.CanPurchase(GarageUpgradeType.Weapon);
            if (armorUpgradeButton != null) armorUpgradeButton.interactable = progress.CanPurchase(GarageUpgradeType.Armor);
        }

        private void GoToMission()
        {
            LeaveGarage(battleSceneName);
        }

        private void BackToMenu()
        {
            LeaveGarage(startMenuSceneName);
        }

        private void LeaveGarage(string sceneName)
        {
            if (isLeavingGarage) return;
            isLeavingGarage = true;
            GarageHubPresentation presentation = GetComponent<GarageHubPresentation>();
            presentation?.PlaySelectionConfirmSound();

            void LoadDestination()
            {
                if (!GameSession.Instance.SceneNavigator.ExitGarage(
                        sceneName,
                        openingDoor: sceneName == battleSceneName,
                        showCarInTransit: true)
                    && !GameSession.Instance.SceneNavigator.TryLoad(sceneName))
                {
                    isLeavingGarage = false;
                    Debug.LogWarning($"Cannot load '{sceneName}'. Check that its scene is present in Build Settings.", this);
                }
            }

            if (presentation != null) presentation.PlayExitAnimation(LoadDestination);
            else LoadDestination();
        }
    }
}
