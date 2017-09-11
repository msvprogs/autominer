using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IExchangeTraderFactory
    {
        IExchangeTrader Create(ExchangeType exchangeType);
    }
}
