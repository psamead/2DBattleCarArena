using System.Collections;
using BattleCarArena.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BattleCarArena.UI
{
    public sealed class GarageDoorTransitController : MonoBehaviour
    {
        [SerializeField] private GarageDoorTransitionTheme theme;
        [SerializeField] private Image garageBackgroundImage;
        [SerializeField] private Image garageCarImage;
        [SerializeField] private Image interiorDarknessImage;
        [SerializeField] private Image doorPanelImage;
        [SerializeField] private Image doorFrameImage;
        [SerializeField, Min(0.1f)] private float openingAnimationDuration = 2f;
        [SerializeField, Min(0.1f)] private float missionOpeningAnimationDuration = 2.55f;
        [SerializeField, Min(0.1f)] private float closingAnimationDuration = 2.55f;
        [SerializeField, Range(0f, 1f)] private float closedInteriorDarkness = 0.85f;
        [SerializeField, Range(0f, 1f)] private float openInteriorDarkness = 0.2f;
        private Vector2 closedPanelPosition;
        [SerializeField] private float panelOpenTravel = 730f;

        private string destinationSceneName;
        private bool opening;
        private bool showCar;

        public void Configure(
            GarageDoorTransitionTheme transitionTheme,
            Image garageBackground,
            Image garageCar,
            Image interiorDarkness,
            Image doorPanel,
            Image doorFrame)
        {
            theme = transitionTheme;
            garageBackgroundImage = garageBackground;
            garageCarImage = garageCar;
            interiorDarknessImage = interiorDarkness;
            doorPanelImage = doorPanel;
            doorFrameImage = doorFrame;
        }

        private void Start()
        {
            if (doorPanelImage != null)
            {
                // Use the authored scene placement so Inspector adjustments define the closed pose.
                closedPanelPosition = doorPanelImage.rectTransform.anchoredPosition;
            }

            if (!GameSession.Instance.TryConsumeGarageTransit(out destinationSceneName, out opening, out showCar))
            {
                Debug.LogError("GarageTransit loaded without a pending door transition request.", this);
                SceneManager.LoadScene("GarageHub", LoadSceneMode.Single);
                return;
            }

            ApplyTheme();
            StartCoroutine(opening ? OpenGarage() : CloseGarage());
        }

        private void ApplyTheme()
        {
            if (theme != null)
            {
                if (garageBackgroundImage != null) garageBackgroundImage.sprite = theme.GarageBackgroundSprite;
                if (garageCarImage != null) garageCarImage.sprite = theme.GarageCarSprite;
                if (doorPanelImage != null) doorPanelImage.sprite = theme.PanelSprite;
                if (doorFrameImage != null) doorFrameImage.sprite = theme.FrameSprite;
            }

            if (garageCarImage != null)
            {
                garageCarImage.gameObject.SetActive(showCar && garageCarImage.sprite != null);
            }

            if (doorPanelImage != null)
            {
                doorPanelImage.rectTransform.anchoredPosition = opening
                    ? closedPanelPosition
                    : closedPanelPosition + Vector2.up * panelOpenTravel;
            }

            SetDarkness(opening ? closedInteriorDarkness : openInteriorDarkness);
        }

        private IEnumerator OpenGarage()
        {
            float duration = destinationSceneName == "BattleArena"
                ? missionOpeningAnimationDuration
                : openingAnimationDuration;
            yield return AnimateDoor(closedPanelPosition, closedPanelPosition + Vector2.up * panelOpenTravel, true, duration);
            GameSession.Instance.MarkGarageEntrancePresentationPending();
            LoadDestination();
        }

        private IEnumerator CloseGarage()
        {
            yield return AnimateDoor(closedPanelPosition + Vector2.up * panelOpenTravel, closedPanelPosition, false, closingAnimationDuration);
            LoadDestination();
        }

        private IEnumerator AnimateDoor(Vector2 startPosition, Vector2 endPosition, bool openingDoor, float duration)
        {
            if (doorPanelImage == null || interiorDarknessImage == null)
            {
                Debug.LogError("GarageTransit is missing its door panel or darkness overlay.", this);
                yield break;
            }

            float elapsed = 0f;
            float startDarkness = openingDoor ? closedInteriorDarkness : openInteriorDarkness;
            float endDarkness = openingDoor ? openInteriorDarkness : closedInteriorDarkness;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = linear * linear * (3f - 2f * linear);
                doorPanelImage.rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
                SetDarkness(Mathf.Lerp(startDarkness, endDarkness, eased));
                yield return null;
            }

            doorPanelImage.rectTransform.anchoredPosition = endPosition;
            SetDarkness(endDarkness);
        }

        private void SetDarkness(float alpha)
        {
            if (interiorDarknessImage == null)
            {
                return;
            }

            Color color = interiorDarknessImage.color;
            color.a = alpha;
            interiorDarknessImage.color = color;
        }

        private void LoadDestination()
        {
            if (!GameSession.Instance.SceneNavigator.TryLoad(destinationSceneName))
            {
                Debug.LogError($"Could not load '{destinationSceneName}' after the garage-door transition.", this);
            }
        }
    }
}
