using Msv.AutoMiner.Common.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Msv.AutoMiner.Common.Models.CoinInfoService
{
    public class MarketPriceData
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ExchangeType Exchange { get; set; }

        public double BtcPerDay { get; set; }

        public double UsdPerDay { get; set; }

        public double LastDayVolume { get; set; }
    }
}
