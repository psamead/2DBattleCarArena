using UnityEngine;

namespace BattleCarArena.Battle
{
    public sealed class BattleArenaBoundary : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            other.GetComponent<CrashReporter>()?.ReportBoundaryHit();
        }

    }
}
