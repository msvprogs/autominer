using System;

namespace Msv.AutoMiner.Data.Logic.Data
{
    public class CoinValue
    {
        public Guid CurrencyId { get; set; }
        public double AverageBtcValue { get; set; }
        public CoinExchangePrice[] ExchangePrices { get; set; }
    }
}
