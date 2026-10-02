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
        [SerializeField, Min(0.1f)] private float missionSceneCrossfadeDuration = 1f;
        [SerializeField, Min(0.1f)] private float closingAnimationDuration = 2.55f;
        [SerializeField, Range(0f, 1f)] private float closedInteriorDarkness = 0.85f;
        [SerializeField, Range(0f, 1f)] private float openInteriorDarkness = 0.2f;
        private Vector2 closedPanelPosition;
        [SerializeField] private float panelOpenTravel = 730f;

        private string destinationSceneName;
        private bool opening;
        private bool showCar;
        private bool crossfadeFromGarageHub;
        private Color garageBackgroundBaseColor;
        private Color garageCarBaseColor;
        private Color doorPanelBaseColor;
        private Color doorFrameBaseColor;
        private GarageHubPresentation garageHubPresentation;

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

            if (!GameSession.Instance.TryConsumeGarageTransit(out destinationSceneName, out opening, out showCar, out crossfadeFromGarageHub))
            {
                Debug.LogError("GarageTransit loaded without a pending door transition request.", this);
                SceneManager.LoadScene("GarageHub", LoadSceneMode.Single);
                return;
            }

            ApplyTheme();
            CacheVisualBaseColors();
            if (crossfadeFromGarageHub)
            {
                garageHubPresentation = FindGarageHubPresentation();
                DisableOtherAudioListeners();
                SetLayerAlpha(garageBackgroundImage, garageBackgroundBaseColor, 0f);
                SetLayerAlpha(garageCarImage, garageCarBaseColor, 0f);
                SetLayerAlpha(doorPanelImage, doorPanelBaseColor, 0f);
                SetLayerAlpha(doorFrameImage, doorFrameBaseColor, 0f);
                SetDarkness(0f);
            }
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
            if (destinationSceneName == "GarageHub")
            {
                GameSession.Instance.MarkGarageEntrancePresentationPending();
            }
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
            bool crossfadingMissionExit = crossfadeFromGarageHub && openingDoor && destinationSceneName == "BattleArena";
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / duration);
                float eased = linear * linear * (3f - 2f * linear);
                doorPanelImage.rectTransform.anchoredPosition = Vector2.LerpUnclamped(startPosition, endPosition, eased);
                if (crossfadingMissionExit)
                {
                    ApplyMissionExitCrossfade(elapsed, eased, startDarkness, endDarkness);
                }
                else
                {
                    SetDarkness(Mathf.Lerp(startDarkness, endDarkness, eased));
                }
                yield return null;
            }

            doorPanelImage.rectTransform.anchoredPosition = endPosition;
            SetDarkness(endDarkness);
            if (crossfadingMissionExit)
            {
                SetLayerAlpha(garageBackgroundImage, garageBackgroundBaseColor, 1f);
                SetLayerAlpha(doorPanelImage, doorPanelBaseColor, 1f);
                SetLayerAlpha(doorFrameImage, doorFrameBaseColor, 1f);
                SetLayerAlpha(garageCarImage, garageCarBaseColor, 0f);
                garageHubPresentation?.SetTransitCarFade(0f);
                garageHubPresentation?.SetTransitSceneFade(1f);
            }
        }

        private void ApplyMissionExitCrossfade(float elapsed, float doorProgress, float startDarkness, float endDarkness)
        {
            float fadeProgress = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / missionSceneCrossfadeDuration));
            SetLayerAlpha(garageBackgroundImage, garageBackgroundBaseColor, fadeProgress);
            SetLayerAlpha(doorPanelImage, doorPanelBaseColor, fadeProgress);
            SetLayerAlpha(doorFrameImage, doorFrameBaseColor, fadeProgress);
            SetDarkness(Mathf.Lerp(startDarkness, endDarkness, doorProgress) * fadeProgress);

            garageHubPresentation?.SetTransitSceneFade(fadeProgress);
            float carAlpha = showCar ? fadeProgress * (1f - doorProgress) : 0f;
            SetLayerAlpha(garageCarImage, garageCarBaseColor, carAlpha);
            garageHubPresentation?.SetTransitCarFade(1f - fadeProgress);
        }

        private void CacheVisualBaseColors()
        {
            garageBackgroundBaseColor = garageBackgroundImage != null ? garageBackgroundImage.color : Color.clear;
            garageCarBaseColor = garageCarImage != null ? garageCarImage.color : Color.clear;
            doorPanelBaseColor = doorPanelImage != null ? doorPanelImage.color : Color.clear;
            doorFrameBaseColor = doorFrameImage != null ? doorFrameImage.color : Color.clear;
        }

        private GarageHubPresentation FindGarageHubPresentation()
        {
            Scene garageScene = SceneManager.GetSceneByName("GarageHub");
            if (!garageScene.IsValid() || !garageScene.isLoaded)
                return null;

            foreach (GameObject root in garageScene.GetRootGameObjects())
            {
                GarageHubPresentation presentation = root.GetComponentInChildren<GarageHubPresentation>(true);
                if (presentation != null)
                    return presentation;
            }

            return null;
        }

        private void DisableOtherAudioListeners()
        {
            AudioListenerHandoff.DisableAllEnabled(gameObject.scene);
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
            if (opening && destinationSceneName == "GarageHub")
            {
                StartCoroutine(CrossfadeIntoGarageHub());
                return;
            }

            if (!GameSession.Instance.SceneNavigator.TryLoad(destinationSceneName))
            {
                garageHubPresentation?.SetTransitCarFade(1f);
                Debug.LogError($"Could not load '{destinationSceneName}' after the garage-door transition.", this);
            }
        }

        private IEnumerator CrossfadeIntoGarageHub()
        {
            if (!GameSession.Instance.SceneNavigator.CanLoad(destinationSceneName))
            {
                Debug.LogError($"Could not load '{destinationSceneName}' after the garage-door transition.", this);
                yield break;
            }

            AudioListener[] disabledListeners = AudioListenerHandoff.DisableAllEnabled();

            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(destinationSceneName, LoadSceneMode.Additive);
            if (loadOperation == null)
            {
                AudioListenerHandoff.Restore(disabledListeners);
                Debug.LogError($"Could not begin loading '{destinationSceneName}' additively.", this);
                yield break;
            }

            while (!loadOperation.isDone)
            {
                yield return null;
            }

            Scene garageScene = SceneManager.GetSceneByName(destinationSceneName);
            if (!garageScene.IsValid() || !garageScene.isLoaded)
            {
                AudioListenerHandoff.Restore(disabledListeners);
                Debug.LogError($"'{destinationSceneName}' did not finish loading for the garage crossfade.", this);
                yield break;
            }

            SceneManager.SetActiveScene(garageScene);
            // Let GarageHub's Start method initialize its background, car, and UI at zero alpha.
            yield return null;

            Image[] transitLayers =
            {
                garageBackgroundImage,
                garageCarImage,
                interiorDarknessImage,
                doorPanelImage,
                doorFrameImage
            };
            Color[] originalColors = new Color[transitLayers.Length];
            for (int i = 0; i < transitLayers.Length; i++)
            {
                originalColors[i] = transitLayers[i] != null ? transitLayers[i].color : Color.clear;
            }

            float elapsed = 0f;
            const float duration = 1f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                float eased = progress * progress * (3f - 2f * progress);
                for (int i = 0; i < transitLayers.Length; i++)
                {
                    SetLayerAlpha(transitLayers[i], originalColors[i], 1f - eased);
                }

                yield return null;
            }

            for (int i = 0; i < transitLayers.Length; i++)
            {
                SetLayerAlpha(transitLayers[i], originalColors[i], 0f);
            }

            SceneManager.UnloadSceneAsync(gameObject.scene);
        }

        private static void SetLayerAlpha(Image image, Color originalColor, float alpha)
        {
            if (image == null) return;
            Color color = originalColor;
            color.a *= alpha;
            image.color = color;
        }
    }
}
