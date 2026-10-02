namespace BattleCarArena.Battle
{
    /// <summary>Applies armor power to a limited number of car hits and gun shots per battle.</summary>
    public sealed class BattleArmorDefense
    {
        public const int InitialCarHitBlocks = 3;
        public const int InitialGunShotBlocks = 3;

        public BattleArmorDefense(int armorPower)
        {
            ArmorPower = System.Math.Max(0, armorPower);
            CarHitBlocksRemaining = InitialCarHitBlocks;
            GunShotBlocksRemaining = InitialGunShotBlocks;
        }

        public int ArmorPower { get; }
        public int CarHitBlocksRemaining { get; private set; }
        public int GunShotBlocksRemaining { get; private set; }

        public int AbsorbCarHit(int attackPower)
        {
            int protection = CarHitBlocksRemaining > 0 ? ArmorPower : 0;
            if (CarHitBlocksRemaining > 0) CarHitBlocksRemaining--;
            return System.Math.Max(0, attackPower - protection);
        }

        public int AbsorbGunShot(int attackPower)
        {
            int protection = GunShotBlocksRemaining > 0 ? ArmorPower : 0;
            if (GunShotBlocksRemaining > 0) GunShotBlocksRemaining--;
            return System.Math.Max(0, attackPower - protection);
        }
    }
}
