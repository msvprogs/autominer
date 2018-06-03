using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data
{
    public class Coin : IEntity<Guid>
    {
        [Key]
        public Guid Id { get; set; }

        [Required, MaxLength(256)]
        public string Name { get; set; }

        [Required, MaxLength(32)]
        public string Symbol { get; set; }

        public ActivityState Activity { get; set; }

        public Guid AlgorithmId { get; set; }

        public virtual CoinAlgorithm Algorithm { get; set; }

        public CoinNetworkInfoApiType NetworkInfoApiType { get; set; }

        [MaxLength(512)]
        public string NetworkInfoApiUrl { get; set; }

        [MaxLength(64)]
        public string NetworkInfoApiName { get; set; }

        public bool GetDifficultyFromLastPoWBlock { get; set; }

        [MaxLength(128)]
        public string MaxTarget { get; set; }

        [MaxLength(512)]
        public string NodeHost { get; set; }

        public int NodePort { get; set; }

        [MaxLength(64)]
        public string NodeLogin { get; set; }

        [MaxLength(64)]
        public string NodePassword { get; set; }

        [MaxLength(16384)]
        public string RewardCalculationJavaScript { get; set; }

        [MaxLength(32768)]
        public byte[] LogoImageBytes { get; set; }

        public virtual ICollection<Wallet> Wallets { get; set; }

        public AddressFormat AddressFormat { get; set; }

        [MaxLength(64)]
        public string AddressPrefixes { get; set; }

        public bool IgnoreInactiveMarket { get; set; }

        public CoinLastNetworkInfoResult? LastNetworkInfoResult { get; set; }

        [MaxLength(8192)]
        public string LastNetworkInfoMessage { get; set; }

        public bool DisableBlockRewardChecking { get; set; }
    }
}
