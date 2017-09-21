using Msv.AutoMiner.CoinInfoService.External.Data;

namespace Msv.AutoMiner.CoinInfoService.External.Contracts
{
    public interface IFiatValueProvider
    {
        CurrencyFiatValue[] GetFiatValues();
    }
}
