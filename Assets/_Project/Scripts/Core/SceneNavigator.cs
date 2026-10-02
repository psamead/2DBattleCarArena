using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleCarArena.Core
{
    public sealed class SceneNavigator
    {
        public bool CanLoad(string sceneName)
        {
            return !string.IsNullOrWhiteSpace(sceneName)
                && Application.CanStreamedLevelBeLoaded(sceneName);
        }

        public bool TryLoad(string sceneName)
        {
            if (!CanLoad(sceneName))
            {
                return false;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return true;
        }

        public bool EnterGarage(string garageSceneName, bool showCarInTransit)
        {
            GameSession session = GameSession.Instance;
            if (!session.GarageProgress.HasWonBattle)
            {
                return TryLoad(garageSceneName);
            }

            return LoadThroughGarageTransit(garageSceneName, opening: true, showCar: showCarInTransit);
        }

        public bool ExitGarage(string destinationSceneName, bool openingDoor, bool showCarInTransit)
        {
            if (!GameSession.Instance.GarageProgress.HasWonBattle)
            {
                return TryLoad(destinationSceneName);
            }

            return LoadThroughGarageTransit(destinationSceneName, openingDoor, showCarInTransit);
        }

        private bool LoadThroughGarageTransit(string destinationSceneName, bool opening, bool showCar)
        {
            const string transitSceneName = "GarageTransit";
            if (!CanLoad(destinationSceneName) || !CanLoad(transitSceneName))
            {
                return false;
            }

            bool crossfadeFromGarageHub = opening
                && destinationSceneName == "BattleArena"
                && SceneManager.GetActiveScene().name == "GarageHub";
            GameSession.Instance.SetPendingGarageTransit(destinationSceneName, opening, showCar, crossfadeFromGarageHub);
            if (crossfadeFromGarageHub)
            {
                AudioListener[] disabledListeners = AudioListenerHandoff.DisableAllEnabled();
                try
                {
                    SceneManager.LoadScene(transitSceneName, LoadSceneMode.Additive);
                    return true;
                }
                catch (System.Exception exception)
                {
                    AudioListenerHandoff.Restore(disabledListeners);
                    GameSession.Instance.ClearPendingGarageTransit();
                    Debug.LogError($"Could not load '{transitSceneName}' additively: {exception.Message}");
                    return false;
                }
            }

            if (TryLoad(transitSceneName))
            {
                return true;
            }

            GameSession.Instance.ClearPendingGarageTransit();
            return false;
        }

    }

    internal static class AudioListenerHandoff
    {
        public static AudioListener[] DisableAllEnabled(Scene keepScene = default)
        {
            AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
            List<AudioListener> disabledListeners = new(listeners.Length);
            foreach (AudioListener listener in listeners)
            {
                if (listener == null || !listener.enabled || !listener.gameObject.activeInHierarchy)
                    continue;
                if (keepScene.IsValid() && listener.gameObject.scene == keepScene)
                    continue;

                listener.enabled = false;
                disabledListeners.Add(listener);
            }

            return disabledListeners.ToArray();
        }

        public static void Restore(AudioListener[] listeners)
        {
            if (listeners == null)
                return;

            foreach (AudioListener listener in listeners)
            {
                if (listener != null)
                    listener.enabled = true;
            }
        }
    }
}
