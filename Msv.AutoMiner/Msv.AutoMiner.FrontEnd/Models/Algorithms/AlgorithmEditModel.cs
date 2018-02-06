using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.FrontEnd.Models.Miners;

namespace Msv.AutoMiner.FrontEnd.Models.Algorithms
{
    public class AlgorithmEditModel : AlgorithmBaseModel
    {
        public bool IsNewEntity { get; set; }

        public int? MinerId { get; set; }

        public double? Intensity { get; set; }

        [MaxLength(128)]
        public string AdditionalArguments {get; set; }

        public MinerBaseModel[] AvailableMiners { get; set; }
    }
}
