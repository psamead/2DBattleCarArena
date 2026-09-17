using UnityEngine;

namespace BattleCarArena.Core
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameSession : MonoBehaviour
    {
        private const string ObjectName = "GameSession";
        private static GameSession instance;

        public static GameSession Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameSession>();
                }

                return instance != null ? instance : CreateInstance();
            }
        }

        public static bool HasInstance => instance != null;

        public SceneNavigator SceneNavigator { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            instance = null;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            _ = Instance;
        }

        private static GameSession CreateInstance()
        {
            GameObject sessionObject = new(ObjectName);
            return sessionObject.AddComponent<GameSession>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            SceneNavigator = new SceneNavigator();
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
