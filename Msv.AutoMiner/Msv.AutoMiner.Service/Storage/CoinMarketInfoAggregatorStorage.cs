using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class CoinMarketInfoAggregatorStorage : ICoinMarketInfoAggregatorStorage
    {
        public Coin[] GetCoins()
        {
            using (var context = new AutoMinerDbContext())
                return context.Coins.ToArray();
        }

        public void StoreCurrentPrices(CoinBtcPrice[] prices)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.CoinBtcPrices.AddRange(prices);
                context.SaveChanges();
            }
        }

        public Dictionary<int, double> GetCoinMeanPrices(TimeSpan period)
        {
            using (var context = new AutoMinerDbContext())
            {
                var coinProfitByAskPriceFlags = context.Coins
                    .ToDictionary(x => x.Id, x => x.ProfitByAskPrice);
                var earliest = DateTime.Now - period;
                return context.CoinBtcPrices
                    .Where(x => x.DateTime >= earliest)
                    .GroupBy(x => x.CoinId)
                    .Select(x => new
                    {
                        CoinId = x.Key,
                        MeanAskPrice = x.Average(y => y.LowestAsk),
                        MeanBidPrice = x.Average(y => y.HighestBid)
                    })
                    .ToDictionary(x => x.CoinId, x => coinProfitByAskPriceFlags[x.CoinId] ? x.MeanAskPrice : x.MeanBidPrice);
            }
        }
    }
}
