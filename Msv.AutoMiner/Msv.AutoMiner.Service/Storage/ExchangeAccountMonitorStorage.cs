using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class ExchangeAccountMonitorStorage : IExchangeAccountMonitorStorage
    {
        public int? GetBitCoinId()
        {
            using (var context = new AutoMinerDbContext())
                return context.Coins.FirstOrDefault(x => x.CurrencySymbol == "BTC")?.Id;
        }

        public Coin[] GetCoinsWithExchanges()
        {
            using (var context = new AutoMinerDbContext())
            {
                var exchangesWithKeys = context.Exchanges
                    .Select(x => x.Type)
                    .ToArray();
                return context.Coins
                    .Where(x => x.Exchange != ExchangeType.Unknown)
                    .Where(x => exchangesWithKeys.Contains(x.Exchange))
                    .ToArray();
            }
        }

        public Dictionary<ExchangeType, DateTime> GetLastOperationDates()
        {
            using (var context = new AutoMinerDbContext())
                return context.ExchangeAccountOperations
                    .GroupBy(x => x.Exchange)
                    .Select(x => new
                    {
                        Exchange = x.Key,
                        LastDateItem = x.OrderByDescending(y => y.DateTime).FirstOrDefault()
                    })
                    .Where(x => x.LastDateItem != null)
                    .ToDictionary(x => x.Exchange, x => x.LastDateItem.DateTime);
        }

        public void SaveBalances(ExchangeAccountBalance[] balances)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.ExchangeAccountBalances.AddRange(balances);
                context.SaveChanges();
            }
        }

        public void SaveOperations(ExchangeAccountOperation[] operations)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.ExchangeAccountOperations.AddRange(operations);
                context.SaveChanges();
            }
        }
    }
}
