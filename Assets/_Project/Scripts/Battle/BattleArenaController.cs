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

        [Header("Challenger Blockout Stats")]
        [SerializeField] private string playerName = "PLAYER";
        [SerializeField] private string challengerName = "CHALLENGER";
        [SerializeField, Min(1)] private int challengerEnginePower = 220;
        [SerializeField, Min(1)] private int challengerWeaponDamage = 15;
        [SerializeField, Min(1)] private int challengerArmorDurability = 140;

        [Header("Battle Timing")]
        [SerializeField, Min(0f)] private float startCueSeconds = 2f;
        [SerializeField, Min(0.1f)] private float contactDamageInterval = 0.65f;
        [SerializeField, Min(0f)] private float resultDisplaySeconds = 2.5f;

        private BattleState state;
        private int playerHealth;
        private int challengerHealth;
        private int playerWeaponDamage;
        private int challengerWeaponDamageRuntime;
        private bool carsInContact;
        private float contactTimer;

        private void Awake()
        {
            GarageProgress progress = GameSession.Instance.GarageProgress;
            playerHealth = Mathf.Max(1, progress.ArmorDurability);
            challengerHealth = Mathf.Max(1, challengerArmorDurability);
            playerWeaponDamage = Mathf.Max(1, progress.WeaponDamage);
            challengerWeaponDamageRuntime = Mathf.Max(1, challengerWeaponDamage);

            playerMotor.Configure(progress.EnginePower, 1);
            challengerMotor.Configure(challengerEnginePower, -1);
            playerCrashReporter.Configure(this, BattleSide.Player);
            challengerCrashReporter.Configure(this, BattleSide.Challenger);

            hud.SetNames(playerName, challengerName);
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
            playerMotor.StartDriving();
            challengerMotor.StartDriving();
        }

        private void Update()
        {
            if (state != BattleState.Fighting || !carsInContact)
            {
                return;
            }

            contactTimer += Time.deltaTime;
            if (contactTimer < contactDamageInterval)
            {
                return;
            }

            contactTimer = 0f;
            playerHealth = Mathf.Max(0, playerHealth - challengerWeaponDamageRuntime);
            challengerHealth = Mathf.Max(0, challengerHealth - playerWeaponDamage);
            RefreshHealth();

            if (playerHealth == 0 || challengerHealth == 0)
            {
                Finish(BattleResolver.ResolveHealthDepletion(playerHealth, challengerHealth));
            }
        }

        public void SetCarsInContact(bool isInContact)
        {
            if (state == BattleState.Preparing || state == BattleState.Results)
            {
                return;
            }

            carsInContact = isInContact;
            if (isInContact)
            {
                state = BattleState.Fighting;
            }
            else
            {
                contactTimer = 0f;
                state = BattleState.Approaching;
            }
        }

        public void ReportBoundaryHit(BattleSide sideAtBoundary)
        {
            if (state == BattleState.Preparing || state == BattleState.Results)
            {
                return;
            }

            Finish(BattleResolver.ResolveBoundaryHit(sideAtBoundary));
        }

        private void Finish(BattleResolution resolution)
        {
            if (state == BattleState.Results)
            {
                return;
            }

            state = BattleState.Results;
            carsInContact = false;
            playerMotor.StopDriving();
            challengerMotor.StopDriving();
            hud.SetCue(string.Empty);
            hud.ResultView.Show(resolution);
            StartCoroutine(ReturnToGarageAfterResult());
        }

        private IEnumerator ReturnToGarageAfterResult()
        {
            yield return new WaitForSecondsRealtime(resultDisplaySeconds);

            if (!new SceneNavigator().TryLoad("GarageHub"))
            {
                Debug.LogError("Could not return to GarageHub after the battle. Confirm GarageHub is enabled in Build Settings.", this);
            }
        }

        private void RefreshHealth()
        {
            hud.SetHealth(playerHealth, Mathf.Max(1, GameSession.Instance.GarageProgress.ArmorDurability),
                challengerHealth, Mathf.Max(1, challengerArmorDurability));
        }
    }
}
