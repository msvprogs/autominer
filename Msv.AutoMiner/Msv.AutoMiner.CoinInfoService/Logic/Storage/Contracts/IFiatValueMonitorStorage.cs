using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts
{
    public interface IFiatValueMonitorStorage
    {
        Coin[] GetCoins();
        FiatCurrency[] GetFiatCurrencies();
        void StoreValues(CoinFiatValue[] values);
    }
}