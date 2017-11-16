using System.Linq;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class CoinValueProvider : ICoinValueProvider
    {
        private readonly AutoMinerDbContext m_Context;

        public CoinValueProvider(AutoMinerDbContext context) 
            => m_Context = context;

        public CoinValue[] GetCurrentCoinValues()
        {
            var maxDate = m_Context.ExchangeMarketPrices
                .Select(x => x.DateTime)
                .Max();
            var btc = m_Context.Coins.First(x => x.Symbol == "BTC");
            return m_Context.ExchangeMarketPrices
                .Where(x => x.SourceCoin.Activity != ActivityState.Deleted)
                .Where(x => x.TargetCoin.Symbol == "BTC")
                .Where(x => x.DateTime == maxDate)
                .AsEnumerable()
                .GroupBy(x => x.SourceCoinId)
                .Select(x => new CoinValue
                {
                    CurrencyId = x.Key,
                    BtcValue = x.Average(y => y.LastPrice),
                    Updated = maxDate
                })
                .Concat(new[] {new CoinValue
                {
                    CurrencyId = btc.Id,
                    BtcValue = 1,
                    Updated = maxDate
                }})
                .ToArray();
        }
    }
}
