namespace BattleCarArena.Battle
{
    public readonly struct BattleResolution
    {
        public BattleResolution(BattleSide winner, BattleSide loser, BattleEndReason reason)
        {
            Winner = winner;
            Loser = loser;
            Reason = reason;
        }

        public BattleSide Winner { get; }
        public BattleSide Loser { get; }
        public BattleEndReason Reason { get; }
    }
}
