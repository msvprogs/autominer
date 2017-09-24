using System;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.Rig.Data
{
    public class MiningState
    {
        public Guid CoinId { get; }
        public string Currency { get; }
        public string Name { get; }
        public string Algorithm { get; }
        public long CurrentHashRate { get; }
        public long StoredHashRate { get; }
        public int? AcceptedShares { get; }

        public MiningState(
            CoinMiningData miningData, long? currentHashRate, long? storedHashRate, int? acceptedShares)
        {
            Currency = miningData?.CoinSymbol ?? throw new ArgumentNullException(nameof(miningData));
            CoinId = miningData.CoinId;
            Name = miningData.CoinName;
            Algorithm = miningData.MinerSettings.AlgorithmId.ToString();
            CurrentHashRate = currentHashRate.GetValueOrDefault();
            StoredHashRate = storedHashRate.GetValueOrDefault();
            AcceptedShares = acceptedShares;
        }

        //TODO: algorithm
        public override string ToString()
            => $"{Name} ({Currency}) [{Algorithm}], " 
            + $"hash rate {ConversionHelper.ToHashRateWithUnits(CurrentHashRate)} "
            + $"(stored rate {ConversionHelper.ToHashRateWithUnits(StoredHashRate)}, " 
            + $"diff {ConversionHelper.GetDiffRatioString(StoredHashRate, CurrentHashRate)}), "
            + $"accepted shares: {(AcceptedShares != null ? AcceptedShares.ToString() : "<unknown>")}";
    }
}
