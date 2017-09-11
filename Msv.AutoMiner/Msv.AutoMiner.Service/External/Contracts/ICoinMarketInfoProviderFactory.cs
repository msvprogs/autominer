using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface ICoinMarketInfoProviderFactory
    {
        ICoinMarketInfoProvider Create(ExchangeType exchange);
    }
}