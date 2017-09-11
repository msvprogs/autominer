using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class ConsolidationRouteBuilderStorage : IConsolidationRouteBuilderStorage
    {
        public Dictionary<ExchangeType, double> GetBitCoinBalances()
        {
            using (var context = new AutoMinerDbContext())
            {
                var btc = context.Coins.FirstOrDefault(x => x.CurrencySymbol == "BTC");
                if (btc == null)
                    return new Dictionary<ExchangeType, double>();
                return context.ExchangeAccountBalances
                    .Where(x => x.CoinId == btc.Id)
                    .GroupBy(x => x.Exchange)
                    .Select(x => new
                    {
                        x.Key,
                        Entry = x.OrderByDescending(y => y.DateTime).FirstOrDefault()
                    })
                    .Where(x => x.Entry != null)
                    .ToDictionary(x => x.Key, x => x.Entry.Balance);
            }
        }
    }
}
