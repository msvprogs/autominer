using System;

namespace Msv.AutoMiner.FrontEnd.Data
{
    public class CoinValue
    {
        public Guid CurrencyId { get; set; }
        public double AverageBtcValue { get; set; }
        public CoinExchangePrice[] ExchangePrices { get; set; }
        public DateTime Updated { get; set; }
    }
}
