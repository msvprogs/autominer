using System;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IStoredFiatValueProvider
    {
        TimestampedValue GetLastFiatValue(string currency, string fiatCurrency, DateTime? dateTime);
        TimestampedValue GetLastBtcUsdValue(DateTime? dateTime = null);
    }
}
