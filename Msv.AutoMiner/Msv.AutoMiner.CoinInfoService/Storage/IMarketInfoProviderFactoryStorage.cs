using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public interface IMarketInfoProviderFactoryStorage
    {
        Exchange GetExchange(ExchangeType exchangeType);
    }
}
