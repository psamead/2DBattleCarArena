using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleCarArena.UI
{
    internal static class StartMenuRuntimeBootstrap
    {
        private const string StartMenuSceneName = "StartMenu";
        private const string RuntimeThemePath = "UI/StartMenuTheme";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsureTemplateExists()
        {
            Scene activeScene = SceneManager.GetActiveScene();
            if (!activeScene.IsValid() || activeScene.name != StartMenuSceneName)
            {
                return;
            }

            if (Object.FindFirstObjectByType<StartMenuController>() != null)
            {
                return;
            }

            GameObject root = new("StartMenuTemplate");
            SceneManager.MoveGameObjectToScene(root, activeScene);
            StartMenuTheme theme = Resources.Load<StartMenuTheme>(RuntimeThemePath);
            StartMenuTemplateFactory.Build(root, theme);
        }
    }
}

