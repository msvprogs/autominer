using System.Threading.Tasks;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IStoredFiatValueProvider
    {
        Task<TimestampedValue> GetLastFiatValueAsync(string currency, string fiatCurrency);
        Task<TimestampedValue> GetLastBtcUsdValueAsync();
    }
}
