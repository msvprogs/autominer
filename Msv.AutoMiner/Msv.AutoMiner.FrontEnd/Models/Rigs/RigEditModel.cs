using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Rigs
{
    public class RigEditModel : RigBaseModel
    {
        public ValueAggregationType DifficultyAggregationType { get; set; }

        public ValueAggregationType PriceAggregationType { get; set; }
    }
}
