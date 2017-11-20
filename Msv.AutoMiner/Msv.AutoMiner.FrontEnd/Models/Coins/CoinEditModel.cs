using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinEditModel : CoinBaseModel
    {
        public double? CanonicalBlockReward { get; set; }

        public double? CanonicalBlockTimeSeconds { get; set; }

        public CoinNetworkInfoApiType NetworkInfoApiType { get; set; }

        public string NetworkApiName { get; set; }

        [Url(ErrorMessage = "Invalid network API URL")]
        public string NetworkApiUrl { get; set; }

        [Url(ErrorMessage = "Invalid node URL")]
        public string NodeUrl { get; set; }

        public string NodeLogin { get; set; }

        public string NodePassword { get; set; }

        [HexNumber(ErrorMessage = "Max target should be a hex number")]
        public string MaxTarget { get; set; }

        [Required(ErrorMessage = "Algorithm isn't chosen")]
        public Guid? AlgorithmId { get; set; }

        public AlgorithmModel[] AvailableAlgorithms { get; set; }
    }
}
