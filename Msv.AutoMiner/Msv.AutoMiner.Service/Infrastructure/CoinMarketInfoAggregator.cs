using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using NLog;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class CoinMarketInfoAggregator : ICoinMarketInfoAggregator
    {
        private const string LtcSymbol = "LTC";
        private static readonly TimeSpan M_PriceMeanPeriod = TimeSpan.FromDays(3);
        private static readonly ILogger M_Logger = LogManager.GetLogger("CoinMarketInfoAggregator");

        private readonly ICoinMarketInfoAggregatorStorage m_Storage;

        public CoinMarketInfoAggregator(ICoinMarketInfoAggregatorStorage storage)
        {
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            m_Storage = storage;
        }

        public Dictionary<string, double> GetAggregatedMarketPrices(
            Dictionary<ExchangeType, Dictionary<string, CoinMarketInfo>> currentMarketValues)
        {
            if (currentMarketValues == null)
                throw new ArgumentNullException(nameof(currentMarketValues));

            var coins = m_Storage.GetCoins().ToDictionary(x => x.Id);

            var now = DateTime.Now;
            var newPrices = coins.Select(x => new
                {
                    Coin = x.Value,
                    MarketInfo = currentMarketValues.TryGetValue(x.Value.Exchange)?.TryGetValue(x.Value.CurrencySymbol)
                })
                .Where(x => x.MarketInfo != null)
                .Select(x => new CoinBtcPrice
                {
                    CoinId = x.Coin.Id,
                    DateTime = now,
                    HighestBid = (x.MarketInfo.BtcHighestBid > 0
                            ? x.MarketInfo.BtcHighestBid
                            : x.MarketInfo.LtcHighestBid * currentMarketValues.TryGetValue(x.Coin.Exchange)?
                                .TryGetValue(LtcSymbol)?.BtcHighestBid)
                        .GetValueOrDefault(),
                    LowestAsk = (x.MarketInfo.BtcLowestAsk > 0
                            ? x.MarketInfo.BtcLowestAsk
                            : x.MarketInfo.LtcLowestAsk * currentMarketValues.TryGetValue(x.Coin.Exchange)?
                                  .TryGetValue(LtcSymbol)?.BtcLowestAsk)
                        .GetValueOrDefault()
                })
                .ToArray();
            m_Storage.StoreCurrentPrices(newPrices);
            M_Logger.Debug($"Market prices were updated. Calculating mean values for past {M_PriceMeanPeriod.TotalDays:F0} days...");
            return m_Storage.GetCoinMeanPrices(M_PriceMeanPeriod)
                .Select(x => new
                {
                    Symbol = coins[x.Key].CurrencySymbol,
                    x.Value
                })
                .GroupBy(x => x.Symbol)
                .ToDictionary(x => x.Key, x => x.First().Value);
        }
    }
}
