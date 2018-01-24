using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Coin
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Symbol { get; set; }

        public ActivityState Activity { get; set; }

        public Guid AlgorithmId { get; set; }

        public virtual CoinAlgorithm Algorithm { get; set; }

        public CoinNetworkInfoApiType NetworkInfoApiType { get; set; }

        public string NetworkInfoApiUrl { get; set; }

        public string NetworkInfoApiName { get; set; }

        public bool GetDifficultyFromLastPoWBlock { get; set; }

        public string MaxTarget { get; set; }

        public string NodeHost { get; set; }

        public int NodePort { get; set; }

        public string NodeLogin { get; set; }

        public string NodePassword { get; set; }

        public double? CanonicalBlockReward { get; set; }

        public double? CanonicalBlockTimeSeconds { get; set; }

        public string RewardCalculationJavaScript { get; set; }

        public byte[] LogoImageBytes { get; set; }

        public virtual ICollection<Wallet> Wallets { get; set; }

        public AddressFormat AddressFormat { get; set; }

        public string AddressPrefixes { get; set; }
    }
}
