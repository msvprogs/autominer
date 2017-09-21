using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface IFiatValueProviderFactory
    {
        IFiatValueProvider Create(CoinFiatValueSource source);
    }
}
