using Msv.AutoMiner.Common.Data.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class ProfitabilityRequest
    {
        public AlgorithmPowerData[] AlgorithmDatas { get; set; }

        public double ElectricityCostUsd { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ValueAggregationType DifficultyAggregationType { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ValueAggregationType PriceAggregationType { get; set; }
    }
}
