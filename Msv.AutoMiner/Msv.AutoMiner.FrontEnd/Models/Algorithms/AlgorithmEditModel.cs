using Msv.AutoMiner.FrontEnd.Models.Miners;

namespace Msv.AutoMiner.FrontEnd.Models.Algorithms
{
    public class AlgorithmEditModel : AlgorithmBaseModel
    {
        public bool IsNewEntity { get; set; }

        public int? MinerId { get; set; }

        public double? Intensity { get; set; }

        public string AdditionalArguments {get; set; }

        public MinerBaseModel[] AvailableMiners { get; set; }
    }
}
