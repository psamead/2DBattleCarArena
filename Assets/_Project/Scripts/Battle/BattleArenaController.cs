using System.Collections;
using BattleCarArena.Core;
using UnityEngine;

namespace BattleCarArena.Battle
{
    public sealed class BattleArenaController : MonoBehaviour
    {
        private enum BattleState
        {
            Preparing,
            Approaching,
            Fighting,
            Results
        }

        [Header("Scene References")]
        [SerializeField] private CarMotor2D playerMotor;
        [SerializeField] private CarMotor2D challengerMotor;
        [SerializeField] private CrashReporter playerCrashReporter;
        [SerializeField] private CrashReporter challengerCrashReporter;
        [SerializeField] private BattleHudView hud;
        [SerializeField] private BattleArenaCameraRig cameraRig;
        [SerializeField] private AudioClip engineSoundClip;
        [SerializeField, Range(0f, 1f)] private float engineStartupVolume = 0.8f;
        [SerializeField, Range(0f, 1f)] private float engineContactVolume = 0.65f;
        [SerializeField, Min(0.05f)] private float minimumEngineImpactClipSeconds = 0.25f;
        [SerializeField, Min(0.05f)] private float maximumEngineImpactClipSeconds = 0.85f;
        [SerializeField] private AudioClip battleEngineBedClip;
        [SerializeField, Range(0f, 1f)] private float battleEngineBedVolume = 0.22f;
        [SerializeField, Range(0f, 1f)] private float battleEngineBedResultVolume = 0.04f;
        [SerializeField, Min(0.05f)] private float battleEngineBedFadeInSeconds = 0.6f;
        [SerializeField, Min(0.05f)] private float battleEngineBedFadeOutSeconds = 0.75f;

        [Header("Challenger Blockout Stats")]
        [SerializeField] private string playerName = "PLAYER";
        [SerializeField] private string challengerName = "CHALLENGER";
        [SerializeField, Range(0f, 0.5f)] private float challengerStatVariation = 0.2f;

        [Header("Battle Timing")]
        [SerializeField, Min(0f)] private float startCueSeconds = 2f;
        [SerializeField, Min(0.1f)] private float contactDamageInterval = 5.5f;
        [SerializeField, Min(0.1f)] private float gunAttackInterval = 5f;
        [SerializeField, Min(0)] private int boundaryCrashDamage = 2;
        private GarageProgress garageProgress;
        private BattleState state;
        private int playerHealth;
        private int playerMaximumHealth;
        private int challengerHealth;
        private int challengerMaximumHealth;
        private int playerWeaponDamage;
        private int challengerWeaponDamageRuntime;
        private int playerEnginePower;
        private int challengerEnginePowerRuntime;
        private BattleArmorDefense playerArmorDefense;
        private BattleArmorDefense challengerArmorDefense;
        private bool carsInContact;
        private float contactTimer;
        private float pushCycleTimer;
        private float gunAttackTimer;
        private bool playerHasPushSurge;
        private BattleCarPresentation[] presentations;
        private AudioSource battleMusicSource;
        private AudioSource engineStartupSource;
        private AudioSource engineImpactSource;
        private float engineImpactRemaining;
        private AudioSource battleEngineBedSource;
        private Coroutine battleEngineBedFadeRoutine;

        private void Awake()
        {
            battleMusicSource = GetComponent<AudioSource>();
            if (battleMusicSource != null)
            {
                battleMusicSource.playOnAwake = false;
                battleMusicSource.loop = true;
                battleMusicSource.spatialBlend = 0f;
            }

            engineStartupSource = CreateEngineAudioSource(1f);
            engineImpactSource = CreateEngineAudioSource(engineContactVolume);
            if (engineImpactSource != null)
                engineImpactSource.clip = engineSoundClip;
            battleEngineBedSource = CreateEngineAudioSource(0f);
            if (battleEngineBedSource != null)
            {
                battleEngineBedSource.clip = battleEngineBedClip;
                battleEngineBedSource.loop = true;
            }

            BattleCarPresentation playerPresentation = playerMotor.GetComponent<BattleCarPresentation>();
            if (playerPresentation == null) playerPresentation = playerMotor.gameObject.AddComponent<BattleCarPresentation>();
            BattleCarPresentation challengerPresentation = challengerMotor.GetComponent<BattleCarPresentation>();
            if (challengerPresentation == null) challengerPresentation = challengerMotor.gameObject.AddComponent<BattleCarPresentation>();
            presentations = new[] { playerPresentation, challengerPresentation };

            garageProgress = GameSession.Instance.GarageProgress;
            playerMaximumHealth = Mathf.Max(1, garageProgress.Score);
            playerHealth = playerMaximumHealth;
            challengerMaximumHealth = RandomizeChallengerStat(playerMaximumHealth);
            challengerHealth = challengerMaximumHealth;
            playerEnginePower = Mathf.Max(1, garageProgress.EnginePower);
            challengerEnginePowerRuntime = RandomizeChallengerStat(playerEnginePower);
            playerWeaponDamage = Mathf.Max(1, garageProgress.WeaponDamage);
            challengerWeaponDamageRuntime = RandomizeChallengerStat(playerWeaponDamage);
            playerArmorDefense = new BattleArmorDefense(garageProgress.ArmorPower);
            challengerArmorDefense = new BattleArmorDefense(RandomizeChallengerStat(garageProgress.ArmorPower));

            playerMotor.Configure(playerEnginePower, 1);
            challengerMotor.Configure(challengerEnginePowerRuntime, -1);
            playerCrashReporter.Configure(this, BattleSide.Player);
            challengerCrashReporter.Configure(this, BattleSide.Challenger);

            hud.SetNames(playerName, challengerName);
            if (hud.ResultView != null)
            {
                hud.ResultView.Confirmed += ReturnToGarage;
            }

            RefreshHealth();
            hud.ResultView.Hide();
            hud.SetCue("GET READY");
            state = BattleState.Preparing;
        }

        private IEnumerator Start()
        {
            if (engineSoundClip != null)
                engineStartupSource?.PlayOneShot(engineSoundClip, engineStartupVolume);

            yield return new WaitForSeconds(startCueSeconds);
            if (state != BattleState.Preparing)
            {
                yield break;
            }

            state = BattleState.Approaching;
            hud.SetCue("FIGHT!");
            battleMusicSource?.Play();
            if (battleEngineBedClip != null && battleEngineBedSource != null)
            {
                battleEngineBedSource.Play();
                FadeBattleEngineBedTo(battleEngineBedVolume, battleEngineBedFadeInSeconds);
            }
            cameraRig?.SetBattleStarted();
            playerMotor.StartDriving();
            challengerMotor.StartDriving();
            foreach (BattleCarPresentation presentation in presentations)
                presentation?.SetBattleSmoke(true);
        }

        private void Update()
        {
            UpdateEngineImpactAudio();

            if (state != BattleState.Preparing && state != BattleState.Results)
            {
                gunAttackTimer += Time.deltaTime;
                if (gunAttackTimer >= gunAttackInterval)
                {
                    gunAttackTimer %= gunAttackInterval;
                    FireGuns();
                    if (state == BattleState.Results) return;
                }

            }

            if (state != BattleState.Fighting || !carsInContact)
            {
                return;
            }

            pushCycleTimer += Time.deltaTime;
            if (pushCycleTimer >= 1.6f)
            {
                pushCycleTimer -= 1.6f;
                playerHasPushSurge = !playerHasPushSurge;
                playerMotor.SetContactPushMultiplier(playerHasPushSurge ? 1.08f : 0.92f);
                challengerMotor.SetContactPushMultiplier(playerHasPushSurge ? 0.92f : 1.08f);
                foreach (BattleCarPresentation presentation in presentations)
                    presentation?.TriggerCrash();
                PlayEngineImpactSound();
                TriggerImpact(0.95f);
            }

            contactTimer += Time.deltaTime;
            if (contactTimer < contactDamageInterval)
            {
                return;
            }

            contactTimer -= contactDamageInterval;
            TriggerImpact(0.75f);
            ResolveCarHits();

        }

        public void SetCarsInContact(bool isInContact)
        {
            if (state == BattleState.Preparing || state == BattleState.Results)
            {
                return;
            }

            bool wasInContact = carsInContact;
            carsInContact = isInContact;
            if (presentations != null)
                foreach (BattleCarPresentation presentation in presentations)
                    presentation?.SetInContact(isInContact);
            if (isInContact)
            {
                state = BattleState.Fighting;
                if (!wasInContact)
                {
                    contactTimer = 0f;
                    pushCycleTimer = 0f;
                    playerHasPushSurge = false;
                    playerMotor.SetContactPushMultiplier(0.92f);
                    challengerMotor.SetContactPushMultiplier(1.08f);
                    PlayEngineImpactSound();
                    foreach (BattleCarPresentation presentation in presentations)
                        presentation?.TriggerCrash();
                    TriggerImpact();
                }
            }
            else
            {
                StopEngineImpactSound();
                contactTimer = 0f;
                pushCycleTimer = 0f;
                playerMotor.SetContactPushMultiplier(1f);
                challengerMotor.SetContactPushMultiplier(1f);
                state = BattleState.Approaching;
            }
        }

        public void ReportBoundaryHit(BattleSide sideAtBoundary)
        {
            if (state == BattleState.Preparing || state == BattleState.Results)
            {
                return;
            }

            TriggerImpact(1.8f);
            PlayEngineImpactSound();
            bool playerHitWall = sideAtBoundary == BattleSide.Player;
            CarMotor2D motor = playerHitWall ? playerMotor : challengerMotor;
            motor.ReboundFromWall(motor.transform.position.x < 0f ? 1 : -1);
            BattleCarPresentation presentation = playerHitWall ? presentations[0] : presentations[1];
            presentation?.TriggerCrash();
            if (playerHitWall)
                playerHealth = Mathf.Max(0, playerHealth - boundaryCrashDamage);
            else
                challengerHealth = Mathf.Max(0, challengerHealth - boundaryCrashDamage);
            RefreshHealth();
            if (playerHealth == 0 || challengerHealth == 0)
                Finish(BattleResolver.ResolveHealthDepletion(playerHealth, challengerHealth));
        }

        private void Finish(BattleResolution resolution)
        {
            if (state == BattleState.Results)
            {
                return;
            }

            state = BattleState.Results;
            StopEngineImpactSound();
            FadeBattleEngineBedTo(battleEngineBedResultVolume, battleEngineBedFadeOutSeconds);
            garageProgress.CompleteBattleRound(resolution.Winner == BattleSide.Player);
            carsInContact = false;
            playerMotor.SetContactPushMultiplier(1f);
            challengerMotor.SetContactPushMultiplier(1f);
            cameraRig?.SetBattleFinished();
            playerMotor.StopDriving();
            challengerMotor.StopDriving();
            foreach (BattleCarPresentation presentation in presentations)
                presentation?.StopBattleDust();
            presentations[(int)resolution.Winner]?.PlayDefeatEffects(false);
            presentations[(int)resolution.Loser]?.PlayDefeatEffects(true);
            hud.SetCue(string.Empty);
            hud.ResultView.Show(resolution);
        }

        private void TriggerImpact(float intensity = 1f)
        {
            cameraRig?.TriggerImpact(intensity);
            hud?.TriggerImpact(intensity);
        }

        private void ReturnToGarage()
        {
            if (state != BattleState.Results)
            {
                return;
            }

            if (!GameSession.Instance.SceneNavigator.EnterGarage("GarageHub", showCarInTransit: false))
            {
                Debug.LogError("Could not return to GarageHub after result confirmation. Confirm GarageHub and GarageTransit are enabled in Build Settings.", this);
            }
        }

        private void OnDestroy()
        {
            if (hud != null && hud.ResultView != null)
            {
                hud.ResultView.Confirmed -= ReturnToGarage;
            }
        }

        private void RefreshHealth()
        {
            hud.SetHealth(playerHealth, playerMaximumHealth, challengerHealth, challengerMaximumHealth);
        }

        private AudioSource CreateEngineAudioSource(float volume)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;
            source.volume = volume;
            source.priority = 96;
            return source;
        }

        private void PlayEngineImpactSound()
        {
            if (engineImpactSource == null || engineSoundClip == null)
                return;

            engineImpactSource.Stop();
            engineImpactSource.timeSamples = 0;
            engineImpactSource.Play();
            float minimumDuration = Mathf.Min(minimumEngineImpactClipSeconds, maximumEngineImpactClipSeconds);
            float maximumDuration = Mathf.Max(minimumEngineImpactClipSeconds, maximumEngineImpactClipSeconds);
            float duration = Random.Range(minimumDuration, maximumDuration);
            engineImpactRemaining = Mathf.Min(duration, engineSoundClip.length);
        }

        private void UpdateEngineImpactAudio()
        {
            if (engineImpactRemaining <= 0f)
                return;

            engineImpactRemaining -= Time.unscaledDeltaTime;
            if (engineImpactRemaining <= 0f)
                StopEngineImpactSound();
        }

        private void StopEngineImpactSound()
        {
            engineImpactRemaining = 0f;
            engineImpactSource?.Stop();
        }

        private void FadeBattleEngineBedTo(float targetVolume, float duration)
        {
            if (battleEngineBedSource == null)
                return;

            if (battleEngineBedFadeRoutine != null)
                StopCoroutine(battleEngineBedFadeRoutine);

            battleEngineBedFadeRoutine = StartCoroutine(FadeBattleEngineBedVolume(targetVolume, duration));
        }

        private IEnumerator FadeBattleEngineBedVolume(float targetVolume, float duration)
        {
            float startVolume = battleEngineBedSource.volume;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                battleEngineBedSource.volume = Mathf.Lerp(startVolume, targetVolume, progress);
                yield return null;
            }

            battleEngineBedSource.volume = targetVolume;
            battleEngineBedFadeRoutine = null;
        }

        private int RandomizeChallengerStat(int playerValue)
        {
            int minimum = Mathf.Max(1, Mathf.RoundToInt(playerValue * (1f - challengerStatVariation)));
            int maximum = Mathf.Max(minimum, Mathf.RoundToInt(playerValue * (1f + challengerStatVariation)));
            return Random.Range(minimum, maximum + 1);
        }

        private void FireGuns()
        {
            TriggerImpact(0.35f);
            int damageToChallenger = challengerArmorDefense.AbsorbGunShot(playerWeaponDamage);
            int damageToPlayer = playerArmorDefense.AbsorbGunShot(challengerWeaponDamageRuntime);
            challengerHealth = Mathf.Max(0, challengerHealth - damageToChallenger);
            playerHealth = Mathf.Max(0, playerHealth - damageToPlayer);
            RefreshHealth();

            if (playerHealth == 0 || challengerHealth == 0)
            {
                Finish(BattleResolver.ResolveHealthDepletion(playerHealth, challengerHealth));
            }
        }

        private void ResolveCarHits()
        {
            int damageToChallenger = challengerArmorDefense.AbsorbCarHit(playerEnginePower);
            int damageToPlayer = playerArmorDefense.AbsorbCarHit(challengerEnginePowerRuntime);
            challengerHealth = Mathf.Max(0, challengerHealth - damageToChallenger);
            playerHealth = Mathf.Max(0, playerHealth - damageToPlayer);

            RefreshHealth();
            if (playerHealth == 0 || challengerHealth == 0)
            {
                Finish(BattleResolver.ResolveHealthDepletion(playerHealth, challengerHealth));
            }
        }
    }
}
