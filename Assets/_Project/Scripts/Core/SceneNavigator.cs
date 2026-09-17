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
    }
}
