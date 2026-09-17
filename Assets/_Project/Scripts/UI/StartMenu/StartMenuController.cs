using BattleCarArena.Core;
using UnityEngine;

namespace BattleCarArena.UI
{
    public sealed class StartMenuController : MonoBehaviour
    {
        [SerializeField] private StartMenuView view;
        [SerializeField] private StartMenuTheme theme;
        [SerializeField] private string garageSceneName = "GarageHub";

        public void Configure(StartMenuView menuView, StartMenuTheme menuTheme, string destinationScene)
        {
            view = menuView;
            theme = menuTheme;
            garageSceneName = destinationScene;

            if (view != null)
            {
                view.ApplyTheme(theme);
                view.PlayMusic();
            }
        }

        private void Awake()
        {
            if (view == null)
            {
                view = GetComponent<StartMenuView>();
            }
        }

        private void OnEnable()
        {
            if (view == null || view.StartButton == null || view.ExitButton == null)
            {
                Debug.LogError("Start Menu references are incomplete. Rebuild the template from the Tools menu.", this);
                return;
            }

            view.ApplyTheme(theme);
            view.StartButton.onClick.AddListener(StartGame);
            view.ExitButton.onClick.AddListener(ExitGame);
            view.PlayMusic();
            view.SelectPrimaryAction();
        }

        private void Start()
        {
            // EventSystem.current is not guaranteed to be ready during OnEnable.
            view?.SelectPrimaryAction();
        }

        private void OnDisable()
        {
            if (view == null)
            {
                return;
            }

            if (view.StartButton != null)
            {
                view.StartButton.onClick.RemoveListener(StartGame);
            }

            if (view.ExitButton != null)
            {
                view.ExitButton.onClick.RemoveListener(ExitGame);
            }
        }

        private void StartGame()
        {
            if (!GameSession.Instance.SceneNavigator.CanLoad(garageSceneName))
            {
                Debug.LogWarning($"Cannot open '{garageSceneName}' yet. Create the scene and add it to Build Settings first.", this);
                return;
            }

            view.StartButton.interactable = false;
            GameSession.Instance.SceneNavigator.TryLoad(garageSceneName);
        }

        private static void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
