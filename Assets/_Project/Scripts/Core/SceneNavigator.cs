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

            GameSession.Instance.SetPendingGarageTransit(destinationSceneName, opening, showCar);
            if (TryLoad(transitSceneName))
            {
                return true;
            }

            GameSession.Instance.ClearPendingGarageTransit();
            return false;
        }
    }
}
