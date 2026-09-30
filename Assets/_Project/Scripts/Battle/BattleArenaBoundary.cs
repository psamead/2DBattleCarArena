using UnityEngine;

namespace BattleCarArena.Battle
{
    public sealed class BattleArenaBoundary : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            other.GetComponent<CrashReporter>()?.ReportBoundaryHit();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            CrashReporter reporter = collision.collider.GetComponent<CrashReporter>();
            if (reporter == null)
            {
                reporter = collision.otherCollider.GetComponent<CrashReporter>();
            }

            reporter?.ReportBoundaryHit();
        }
    }
}
