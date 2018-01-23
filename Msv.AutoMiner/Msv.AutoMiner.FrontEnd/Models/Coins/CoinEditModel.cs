using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;

namespace Msv.AutoMiner.FrontEnd.Models.Coins
{
    public class CoinEditModel : CoinExportModel
    {
        [Url(ErrorMessage = "Invalid node URL")]
        public string NodeUrl { get; set; }

        public string NodeLogin { get; set; }

        public string NodePassword { get; set; }

        public AlgorithmModel[] AvailableAlgorithms { get; set; }

        public long? LastHeight { get; set; }

        public double? LastDifficulty { get; set; }

        [Url(ErrorMessage = "Invalid logo URL")]
        public string NewLogoUrl { get; set; }

        public bool DeleteLogo { get; set; }
    }
}
