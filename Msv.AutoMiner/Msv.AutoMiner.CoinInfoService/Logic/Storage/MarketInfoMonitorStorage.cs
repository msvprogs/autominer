using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class MarketInfoMonitorStorage : IMarketInfoMonitorStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public MarketInfoMonitorStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Coin[] GetCoins()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Coins
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public Exchange[] GetExchanges()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Exchanges.ToArray();
        }

        public void StoreExchangeCoins(ExchangeCoin[] coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            using (var context = m_Factory.Create())
            {
                var exchanges = coins.Select(x => x.Exchange)
                    .Distinct()
                    .ToArray();
                context.ExchangeCoins
                    .RemoveRange(context.ExchangeCoins.Where(x => exchanges.Contains(x.Exchange)).ToArray());
                context.SaveChanges();
                context.ExchangeCoins.AddRange(coins);
                context.SaveChanges();
            }
        }

        public void StoreMarketPrices(ExchangeMarketPrice[] prices)
        {
            if (prices == null)
                throw new ArgumentNullException(nameof(prices));

            using (var context = m_Factory.Create())
            {
                context.ExchangeMarketPrices.AddRange(prices);
                context.SaveChanges();
            }
        }
    }
}
