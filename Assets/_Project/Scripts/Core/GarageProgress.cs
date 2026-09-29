namespace BattleCarArena.Core
{
    public enum GarageUpgradeType
    {
        Engine,
        Weapon,
        Armor
    }

    /// <summary>Runtime garage credits, score, and upgraded vehicle stats for one play session.</summary>
    public sealed class GarageProgress
    {
        public int Credits { get; private set; } = 1000;
        public int Score { get; private set; } = 100;
        public int EnginePower { get; private set; } = 120;
        public int WeaponDamage { get; private set; } = 40;
        public int ArmorDurability { get; private set; } = 65;

        public int GetUpgradeCost(GarageUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                GarageUpgradeType.Engine => 250,
                GarageUpgradeType.Weapon => 300,
                GarageUpgradeType.Armor => 275,
                _ => 0
            };
        }

        public int GetUpgradeAmount(GarageUpgradeType upgradeType)
        {
            return upgradeType switch
            {
                GarageUpgradeType.Engine => 145,
                GarageUpgradeType.Weapon => 55,
                GarageUpgradeType.Armor => 82,
                _ => 0
            };
        }

        public bool CanPurchase(GarageUpgradeType upgradeType)
        {
            int cost = GetUpgradeCost(upgradeType);
            return cost > 0 && Credits >= cost;
        }

        public bool TryPurchase(GarageUpgradeType upgradeType)
        {
            if (!CanPurchase(upgradeType))
            {
                return false;
            }

            int amount = GetUpgradeAmount(upgradeType);
            Credits -= GetUpgradeCost(upgradeType);
            Score += amount;

            switch (upgradeType)
            {
                case GarageUpgradeType.Engine:
                    EnginePower += amount;
                    break;
                case GarageUpgradeType.Weapon:
                    WeaponDamage += amount;
                    break;
                case GarageUpgradeType.Armor:
                    ArmorDurability += amount;
                    break;
            }

            return true;
        }
    }
}
