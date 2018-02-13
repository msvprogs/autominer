using System;
using Msv.AutoMiner.Common.Data.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class AlgorithmInfo
    {
        public Guid Id { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public KnownCoinAlgorithm? KnownValue { get; set; }

        public string Name { get; set; }
    }
}
