using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data.Logic.Data
{
    public class CoinExchangePrice
    {
        public ExchangeType Exchange { get; set; }
        public double Price { get; set; }
        public double PriceDelta { get; set; }
        public double UsdPrice { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Volume { get; set; }
        public double VolumeBtc => Volume * Price;
        public DateTime Updated { get; set; }
        public bool IsActive { get; set; }
    }
}
