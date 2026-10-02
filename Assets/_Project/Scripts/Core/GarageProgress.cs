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
        private const int MaximumEnergyPercent = 100;
        private const int EnergyCostPerRound = 20;
        private const int EnergyRecoveryInterval = 3;
        private const int EnergyRecoveryPercent = 20;
        private const int VictoryScoreRewardPercent = 5;
        private const int VictoryCreditReward = 200;

        public int Credits { get; private set; } = 1000;
        public int Score { get; private set; } = 500;
        public int EnginePower { get; private set; } = 100;
        public int WeaponDamage { get; private set; } = 100;
        public int ArmorPower { get; private set; } = 100;
        public int EnergyPercent { get; private set; } = MaximumEnergyPercent;
        public int CompletedRounds { get; private set; }
        public bool HasWonBattle { get; private set; }

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
                    ArmorPower += amount;
                    break;
            }

            return true;
        }

        public void CompleteBattleRound(bool playerWon)
        {
            CompletedRounds++;
            EnergyPercent = System.Math.Max(0, EnergyPercent - EnergyCostPerRound);

            if (CompletedRounds % EnergyRecoveryInterval == 0)
            {
                EnergyPercent = System.Math.Min(MaximumEnergyPercent, EnergyPercent + EnergyRecoveryPercent);
            }

            if (playerWon)
            {
                HasWonBattle = true;
                Credits += VictoryCreditReward;
                Score += System.Math.Max(1, (Score * VictoryScoreRewardPercent + 99) / 100);
            }
        }
    }
}
