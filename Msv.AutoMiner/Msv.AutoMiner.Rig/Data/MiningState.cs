using System;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.Rig.Data
{
    public class MiningState
    {
        public string Currency { get; }
        public Guid CoinId { get; }
        public int PoolId { get; }
        public string Name { get; }
        public string Algorithm { get; }
        public long CurrentHashRate { get; }
        public long StoredHashRate { get; }
        public int? AcceptedShares { get; }
        public int? RejectedShares { get; }

        public MiningState(
            CoinMiningData miningData, long? currentHashRate, long? storedHashRate, int? acceptedShares, int? rejectedShares)
        {
            Currency = miningData.CoinSymbol;
            CoinId = miningData.CoinId;
            PoolId = miningData.PoolData?.Id ?? default;
            Name = miningData.CoinName;
            Algorithm = miningData.MinerSettings.Algorithm.AlgorithmName;
            CurrentHashRate = currentHashRate.GetValueOrDefault();
            StoredHashRate = storedHashRate.GetValueOrDefault();
            AcceptedShares = acceptedShares;
            RejectedShares = rejectedShares;
        }

        public override string ToString()
            => $"{Name} ({Currency}) [{Algorithm}], " 
            + $"hash rate {ConversionHelper.ToHashRateWithUnits(CurrentHashRate)} "
            + $"(stored rate {ConversionHelper.ToHashRateWithUnits(StoredHashRate)}, " 
            + $"diff {ConversionHelper.GetDiffRatioString(StoredHashRate, CurrentHashRate)}), "
            + $"accepted shares: {(AcceptedShares != null ? AcceptedShares.ToString() : "<unknown>")}, "
            + $"rejected shares: {(RejectedShares != null ? RejectedShares.ToString() : "<unknown>")}";
    }
}
