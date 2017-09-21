using System;

namespace Msv.AutoMiner.Data
{
    public class CoinFiatValue
    {
        public Guid CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public int FiatCurrencyId { get; set; }

        public virtual FiatCurrency FiatCurrency { get; set; }

        public DateTime DateTime { get; set; }

        public CoinFiatValueSource Source { get; set; }

        public double Value { get; set; }
    }
}
