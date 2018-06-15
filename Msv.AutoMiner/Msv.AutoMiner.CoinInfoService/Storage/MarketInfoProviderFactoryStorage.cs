using System.Linq;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public class MarketInfoProviderFactoryStorage : IMarketInfoProviderFactoryStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public MarketInfoProviderFactoryStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public CoinAlgorithm[] GetAlgorithms()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.CoinAlgorithms.ToArray();
        }

        public Exchange GetExchange(ExchangeType exchangeType)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Exchanges.FirstOrDefault(x => x.Type == exchangeType);
        }
    }
}
