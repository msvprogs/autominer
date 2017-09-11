using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface ICoinMarketInfoProvider
    {
        CoinMarketInfo[] GetCoinMarketInfos(string[] currencySymbols);
    }
}
