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

        [Header("Challenger Blockout Stats")]
        [SerializeField] private string playerName = "PLAYER";
        [SerializeField] private string challengerName = "CHALLENGER";
        [SerializeField, Min(1)] private int challengerEnginePower = 220;
        [SerializeField, Min(1)] private int challengerWeaponDamage = 6;
        [SerializeField, Min(1)] private int challengerArmorDurability = 140;

        [Header("Battle Timing")]
        [SerializeField, Min(0f)] private float startCueSeconds = 2f;
        [SerializeField, Min(0.1f)] private float contactDamageInterval = 5.5f;
        [SerializeField, Min(1f)] private float battleDurationSeconds = 60f;
        [SerializeField, Min(0)] private int boundaryCrashDamage = 2;
        private BattleState state;
        private int playerHealth;
        private int playerMaximumHealth;
        private int challengerHealth;
        private int challengerMaximumHealth;
        private int playerWeaponDamage;
        private int challengerWeaponDamageRuntime;
        private bool carsInContact;
        private float contactTimer;
        private float pushCycleTimer;
        private float battleElapsed;
        private bool playerHasPushSurge;
        private BattleCarPresentation[] presentations;
        private AudioSource battleMusicSource;

        private void Awake()
        {
            battleMusicSource = GetComponent<AudioSource>();
            if (battleMusicSource != null)
            {
                battleMusicSource.playOnAwake = false;
                battleMusicSource.loop = true;
                battleMusicSource.spatialBlend = 0f;
            }

            BattleCarPresentation playerPresentation = playerMotor.GetComponent<BattleCarPresentation>();
            if (playerPresentation == null) playerPresentation = playerMotor.gameObject.AddComponent<BattleCarPresentation>();
            BattleCarPresentation challengerPresentation = challengerMotor.GetComponent<BattleCarPresentation>();
            if (challengerPresentation == null) challengerPresentation = challengerMotor.gameObject.AddComponent<BattleCarPresentation>();
            presentations = new[] { playerPresentation, challengerPresentation };

            GarageProgress progress = GameSession.Instance.GarageProgress;
            playerMaximumHealth = Mathf.Max(1, progress.ArmorDurability);
            playerHealth = playerMaximumHealth;
            challengerMaximumHealth = Mathf.Max(1, challengerArmorDurability);
            challengerHealth = challengerMaximumHealth;
            playerWeaponDamage = Mathf.Max(1, progress.WeaponDamage);
            challengerWeaponDamageRuntime = Mathf.Max(1, challengerWeaponDamage);

            playerMotor.Configure(progress.EnginePower, 1);
            challengerMotor.Configure(challengerEnginePower, -1);
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
            yield return new WaitForSeconds(startCueSeconds);
            if (state != BattleState.Preparing)
            {
                yield break;
            }

            state = BattleState.Approaching;
            hud.SetCue("FIGHT!");
            battleElapsed = 0f;
            battleMusicSource?.Play();
            cameraRig?.SetBattleStarted();
            playerMotor.StartDriving();
            challengerMotor.StartDriving();
            foreach (BattleCarPresentation presentation in presentations)
                presentation?.SetBattleSmoke(true);
        }

        private void Update()
        {
            if (state != BattleState.Preparing && state != BattleState.Results)
            {
                battleElapsed += Time.deltaTime;
                if (battleElapsed >= battleDurationSeconds)
                {
                    Finish(BattleResolver.ResolveTimeLimit(playerHealth, playerMaximumHealth,
                        challengerHealth, challengerMaximumHealth));
                    return;
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
                TriggerImpact(0.95f);
            }

            contactTimer += Time.deltaTime;
            if (contactTimer < contactDamageInterval)
            {
                return;
            }

            contactTimer -= contactDamageInterval;
            TriggerImpact(0.75f);
            playerHealth = Mathf.Max(0, playerHealth - challengerWeaponDamageRuntime);
            challengerHealth = Mathf.Max(0, challengerHealth - playerWeaponDamage);
            RefreshHealth();

            if (playerHealth == 0 || challengerHealth == 0)
            {
                Finish(BattleResolver.ResolveHealthDepletion(playerHealth, challengerHealth));
                return;
            }

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
                    foreach (BattleCarPresentation presentation in presentations)
                        presentation?.TriggerCrash();
                    TriggerImpact();
                }
            }
            else
            {
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

            if (!new SceneNavigator().TryLoad("GarageHub"))
            {
                Debug.LogError("Could not return to GarageHub after result confirmation. Confirm GarageHub is enabled in Build Settings.", this);
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
    }
}
