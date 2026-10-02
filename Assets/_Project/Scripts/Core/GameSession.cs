using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleCarArena.Core
{
    [DefaultExecutionOrder(-10000)]
    public sealed class GameSession : MonoBehaviour
    {
        private const string ObjectName = "GameSession";
        private static GameSession instance;
        private AudioSource uiAudioSource;
        private AudioListener sessionAudioListener;
        private string pendingGarageTransitDestination;
        private bool pendingGarageTransitOpening;
        private bool pendingGarageTransitShowCar;
        private bool pendingGarageTransitCrossfadeFromHub;
        private bool garageEntrancePresentationPending;

        public static GameSession Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<GameSession>();
                }

                if (instance == null)
                {
                    instance = CreateInstance();
                }

                instance.EnsureInitialized();
                return instance;
            }
        }

        public static bool HasInstance => instance != null;

        public SceneNavigator SceneNavigator { get; private set; }
        public GarageProgress GarageProgress { get; private set; }

        public void PlayUiSound(AudioClip clip)
        {
            if (clip == null)
            {
                return;
            }

            if (uiAudioSource == null)
            {
                uiAudioSource = gameObject.AddComponent<AudioSource>();
                uiAudioSource.playOnAwake = false;
                uiAudioSource.loop = false;
                uiAudioSource.spatialBlend = 0f;
                uiAudioSource.volume = 1f;
            }

            uiAudioSource.PlayOneShot(clip);
        }

        public void SetPendingGarageTransit(string destinationSceneName, bool opening, bool showCar, bool crossfadeFromHub = false)
        {
            pendingGarageTransitDestination = destinationSceneName;
            pendingGarageTransitOpening = opening;
            pendingGarageTransitShowCar = showCar;
            pendingGarageTransitCrossfadeFromHub = crossfadeFromHub;
        }

        public bool TryConsumeGarageTransit(out string destinationSceneName, out bool opening, out bool showCar, out bool crossfadeFromHub)
        {
            destinationSceneName = pendingGarageTransitDestination;
            opening = pendingGarageTransitOpening;
            showCar = pendingGarageTransitShowCar;
            crossfadeFromHub = pendingGarageTransitCrossfadeFromHub;
            pendingGarageTransitDestination = null;
            pendingGarageTransitOpening = false;
            pendingGarageTransitShowCar = false;
            pendingGarageTransitCrossfadeFromHub = false;
            return !string.IsNullOrWhiteSpace(destinationSceneName);
        }

        public void ClearPendingGarageTransit()
        {
            pendingGarageTransitDestination = null;
            pendingGarageTransitOpening = false;
            pendingGarageTransitShowCar = false;
            pendingGarageTransitCrossfadeFromHub = false;
        }

        public void MarkGarageEntrancePresentationPending()
        {
            garageEntrancePresentationPending = true;
        }

        public bool ConsumeGarageEntrancePresentationPending()
        {
            bool pending = garageEntrancePresentationPending;
            garageEntrancePresentationPending = false;
            return pending;
        }

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
            EnsureInitialized();
            DontDestroyOnLoad(gameObject);
            sessionAudioListener = GetComponent<AudioListener>();
            if (sessionAudioListener == null)
            {
                sessionAudioListener = gameObject.AddComponent<AudioListener>();
            }

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            EnforceSingleAudioListener();
        }

        private void EnsureInitialized()
        {
            SceneNavigator ??= new SceneNavigator();
            GarageProgress ??= new GarageProgress();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            EnforceSingleAudioListener();
        }

        private void EnforceSingleAudioListener()
        {
            if (sessionAudioListener == null)
            {
                sessionAudioListener = GetComponent<AudioListener>();
                if (sessionAudioListener == null)
                {
                    sessionAudioListener = gameObject.AddComponent<AudioListener>();
                }
            }

            sessionAudioListener.enabled = true;
            foreach (AudioListener listener in FindObjectsByType<AudioListener>(FindObjectsSortMode.None))
            {
                if (listener != null && listener != sessionAudioListener)
                {
                    listener.enabled = false;
                }
            }
        }
    }
}
