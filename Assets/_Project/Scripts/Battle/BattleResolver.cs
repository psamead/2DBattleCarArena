namespace BattleCarArena.Battle
{
    /// <summary>Maps a single defeat condition to the outcome of the one-on-one battle.</summary>
    public static class BattleResolver
    {
        public static BattleResolution ResolveHealthDepletion(int playerHealth, int challengerHealth)
        {
            if (playerHealth <= 0 && challengerHealth <= 0)
            {
                // Preserve the prototype's existing tie rule when both health bars empty on one tick.
                return new BattleResolution(BattleSide.Player, BattleSide.Challenger, BattleEndReason.HealthDepleted);
            }

            BattleSide loser = playerHealth <= 0 ? BattleSide.Player : BattleSide.Challenger;
            BattleSide winner = loser == BattleSide.Player ? BattleSide.Challenger : BattleSide.Player;
            return new BattleResolution(winner, loser, BattleEndReason.HealthDepleted);
        }

        public static BattleResolution ResolveBoundaryHit(BattleSide sideAtBoundary)
        {
            BattleSide winner = sideAtBoundary == BattleSide.Player ? BattleSide.Challenger : BattleSide.Player;
            return new BattleResolution(winner, sideAtBoundary, BattleEndReason.BoundaryHit);
        }

    }
}
