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

        public bool EnterGarage(string garageSceneName)
        {
            GameSession session = GameSession.Instance;
            if (!session.GarageProgress.HasWonBattle)
            {
                return TryLoad(garageSceneName);
            }

            return LoadThroughGarageTransit(garageSceneName, opening: true);
        }

        public bool ExitGarage(string destinationSceneName)
        {
            return LoadThroughGarageTransit(destinationSceneName, opening: false);
        }

        private bool LoadThroughGarageTransit(string destinationSceneName, bool opening)
        {
            const string transitSceneName = "GarageTransit";
            if (!CanLoad(destinationSceneName) || !CanLoad(transitSceneName))
            {
                return false;
            }

            GameSession.Instance.SetPendingGarageTransit(destinationSceneName, opening);
            if (TryLoad(transitSceneName))
            {
                return true;
            }

            GameSession.Instance.ClearPendingGarageTransit();
            return false;
        }
    }
}
