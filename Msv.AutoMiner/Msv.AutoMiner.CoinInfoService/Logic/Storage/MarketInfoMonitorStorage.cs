using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class MarketInfoMonitorStorage : IMarketInfoMonitorStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public MarketInfoMonitorStorage(AutoMinerDbContext context)
            => m_Context = context;

        public Coin[] GetCoins()
        {
            return m_Context.Coins
                .Where(x => x.Activity != ActivityState.Deleted)
                .ToArray();
        }

        public Exchange[] GetExchanges()
        {
            return m_Context.Exchanges.ToArray();
        }

        public void StoreExchangeCoins(ExchangeCoin[] coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            var exchanges = coins.Select(x => x.Exchange)
                .Distinct()
                .ToArray();
            m_Context.ExchangeCoins
                .RemoveRange(m_Context.ExchangeCoins.Where(x => exchanges.Contains(x.Exchange)).ToArray());
            m_Context.SaveChanges();
            m_Context.ExchangeCoins.AddRange(coins);
            m_Context.SaveChanges();
        }

        public void StoreMarketPrices(ExchangeMarketPrice[] prices)
        {
            if (prices == null)
                throw new ArgumentNullException(nameof(prices));

            m_Context.ExchangeMarketPrices.AddRange(prices);
            m_Context.SaveChanges();
        }
    }
}
