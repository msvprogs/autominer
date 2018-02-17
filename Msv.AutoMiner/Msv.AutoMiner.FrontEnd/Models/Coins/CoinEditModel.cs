using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinEditModel : CoinExportModel
    {
        [Url(ErrorMessage = "Invalid node URL")]
        [MaxLength(512)]
        public string NodeUrl { get; set; }

        [MaxLength(64)]
        public string NodeLogin { get; set; }

        [MaxLength(64)]
        public string NodePassword { get; set; }

        public AlgorithmBaseModel[] AvailableAlgorithms { get; set; }

        public long? LastHeight { get; set; }

        public double? LastDifficulty { get; set; }

        public int? LastMasternodeCount { get; set; }

        public double? LastTotalSupply { get; set; }

        [Url(ErrorMessage = "Invalid logo URL")]
        [MaxLength(512)]
        public string NewLogoUrl { get; set; }

        public bool DeleteLogo { get; set; }

        public string[] HardcodedCoins { get; set; }

        public string[] HardcodedMultiAlgoCoins { get; set; }

        public bool IgnoreInactiveMarket { get; set; }
    }
}
