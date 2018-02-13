using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface IMarketInfoProviderFactory
    {
        IMarketInfoProvider Create(ExchangeType exchange);
    }
}