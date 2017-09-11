using System;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Data
{
    public class CoinMiningInfo
    {
        public string Currency { get; }
        public string Name { get; }
        public CoinAlgorithm Algorithm { get; }
        public long CurrentHashRate { get; }
        public long StoredHashRate { get; }
        public int? AcceptedShares { get; }

        public CoinMiningInfo(
            Coin coin, long? currentHashRate, long? storedHashRate, int? acceptedShares)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            Currency = coin.CurrencySymbol;
            Name = coin.Name;
            Algorithm = coin.Algorithm;
            CurrentHashRate = currentHashRate.GetValueOrDefault();
            StoredHashRate = storedHashRate.GetValueOrDefault();
            AcceptedShares = acceptedShares;
        }

        public override string ToString()
            => $"{Name} ({Currency}) [{Algorithm}], " 
            + $"hash rate {ConversionHelper.ToHashRateWithUnits(CurrentHashRate, Algorithm)} "
            + $"(stored rate {ConversionHelper.ToHashRateWithUnits(StoredHashRate, Algorithm)}, " 
            + $"diff {ConversionHelper.GetDiffRatioString(StoredHashRate, CurrentHashRate)}), "
            + $"accepted shares: {(AcceptedShares != null ? AcceptedShares.ToString() : "<unknown>")}";
    }
}
