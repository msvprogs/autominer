using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Data
{
    public class CoinExchangePrice
    {
        public ExchangeType Exchange { get; set; }
        public double Price { get; set; }
        public double UsdPrice { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public DateTime Updated { get; set; }
    }
}
