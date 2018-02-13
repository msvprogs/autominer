using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Algorithms
{
    public class AlgorithmDisplayModel : AlgorithmBaseModel
    {
        public ActivityState Activity { get; set; }

        public string MinerName { get; set; }
    }
}
