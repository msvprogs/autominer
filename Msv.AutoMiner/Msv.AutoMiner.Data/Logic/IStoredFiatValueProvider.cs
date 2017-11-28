using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IStoredFiatValueProvider
    {
        TimestampedValue GetLastFiatValue(string currency, string fiatCurrency);
        TimestampedValue GetLastBtcUsdValue();
    }
}
