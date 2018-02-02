using System;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using System.Collections.Generic;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Data
{
    public class CoinMiningData : IEquatable<CoinMiningData>
    {
        public Guid CoinId { get; set; }
        public string CoinName { get; set; }
        public string CoinSymbol { get; set; }
        public Guid AlgorithmId { get; set; }
        public KnownCoinAlgorithm? KnownCoinAlgorithm { get; set; }
        public bool BenchmarkMode { get; set; }
        public MinerAlgorithmSetting MinerSettings { get; set; }
        public PoolDataModel PoolData { get; set; }
        public double PowerUsage { get; set; }
        public double UsdPerDayTotal => PoolData.UsdPerDay - PoolData.ElectricityCost;

        public override bool Equals(object obj)
        {
            return Equals(obj as CoinMiningData);
        }

        public bool Equals(CoinMiningData other)
        {
            return other != null &&
                   CoinId.Equals(other.CoinId) &&
                   PoolData.Id == other.PoolData.Id;
        }

        public override int GetHashCode()
        {
            var hashCode = 1202184788;
            hashCode = hashCode * -1521134295 + EqualityComparer<Guid>.Default.GetHashCode(CoinId);
            hashCode = hashCode * -1521134295 + PoolData.Id.GetHashCode();
            return hashCode;
        }

        public string ToCoinsPerDayString() 
            => $"{PoolData.CoinsPerDay,15:F4} {CoinSymbol}";

        public string ToFullNameWithBtcString()
            => $"{CoinName} ({PoolData.Name}) [{PoolData.BtcPerDay:F6} BTC/day]";

        public string ToFullNameString()
            => $"{CoinName} ({PoolData.Name})";
    }
}
