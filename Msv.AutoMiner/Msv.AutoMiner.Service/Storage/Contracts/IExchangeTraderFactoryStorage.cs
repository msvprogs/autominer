using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IExchangeTraderFactoryStorage
    {
        Exchange GetExchange(ExchangeType type);
        string[] GetCurrenciesForExchange(ExchangeType type);
    }
}
