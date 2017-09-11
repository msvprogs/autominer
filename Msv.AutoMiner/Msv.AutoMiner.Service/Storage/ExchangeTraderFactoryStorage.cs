using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class ExchangeTraderFactoryStorage : IExchangeTraderFactoryStorage
    {
        public Exchange GetExchange(ExchangeType type)
        {
            using (var context = new AutoMinerDbContext())
                return context.Exchanges.FirstOrDefault(x => x.Type == type);
        }

        public string[] GetCurrenciesForExchange(ExchangeType type)
        {
            using (var context = new AutoMinerDbContext())
                return context.Coins
                    .Where(x => x.Exchange == type)
                    .Select(x => x.CurrencySymbol)
                    .ToArray();
        }
    }
}
