using UnityEngine;

namespace BattleCarArena.Battle
{
    public sealed class CrashReporter : MonoBehaviour
    {
        private BattleArenaController controller;
        private BattleSide side;

        public void Configure(BattleArenaController owner, BattleSide carSide)
        {
            controller = owner;
            side = carSide;
        }

        public void ReportBoundaryHit()
        {
            controller?.ReportBoundaryHit(side);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleColliders(collision.collider, collision.otherCollider);
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            HandleColliders(collision.collider, collision.otherCollider);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (FindCrashReporter(collision.collider, collision.otherCollider) != null)
            {
                controller?.SetCarsInContact(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponent<BattleArenaBoundary>() != null)
            {
                ReportBoundaryHit();
            }
        }

        private void HandleColliders(Collider2D first, Collider2D second)
        {
            if (FindBoundary(first, second) != null)
            {
                controller?.ReportBoundaryHit(side);
                return;
            }

            CrashReporter otherCar = FindCrashReporter(first, second);
            if (otherCar != null && otherCar != this)
            {
                controller?.SetCarsInContact(true);
            }
        }

        private static BattleArenaBoundary FindBoundary(Collider2D first, Collider2D second)
        {
            BattleArenaBoundary boundary = first != null ? first.GetComponent<BattleArenaBoundary>() : null;
            return boundary != null ? boundary : second != null ? second.GetComponent<BattleArenaBoundary>() : null;
        }

        private static CrashReporter FindCrashReporter(Collider2D first, Collider2D second)
        {
            CrashReporter reporter = first != null ? first.GetComponent<CrashReporter>() : null;
            return reporter != null ? reporter : second != null ? second.GetComponent<CrashReporter>() : null;
        }
    }
}
