using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using NLog;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class MarketValuesProvider : IMarketValuesProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("MarketValuesProvider");
        private readonly ICoinMarketInfoProviderFactory m_InfoProviderFactory;

        public MarketValuesProvider(ICoinMarketInfoProviderFactory infoProviderFactory)
        {
            if (infoProviderFactory == null)
                throw new ArgumentNullException(nameof(infoProviderFactory));

            m_InfoProviderFactory = infoProviderFactory;
        }

        public Dictionary<ExchangeType, Dictionary<string, CoinMarketInfo>> GetCoinMarketValues(Coin[] coins)
        {
            if (coins == null)
                throw new ArgumentNullException(nameof(coins));

            return coins
                .GroupBy(x => x.Exchange)
                .Select(x => new
                {
                    Exchange = x.Key,
                    Provider = m_InfoProviderFactory.Create(x.Key),
                    Currencies = x.Select(y => y.CurrencySymbol).ToArray()
                })
                .Select(x =>
                {
                    try
                    {
                        return new
                        {
                            x.Exchange,
                            Data = x.Provider.GetCoinMarketInfos(x.Currencies)
                        };
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, $"Couldn't obtain current market values for {x.Exchange}");
                        return new { x.Exchange, Data = new CoinMarketInfo[0] };
                    }
                })
                .Where(x => x.Data != null)
                .ToDictionary(
                    x => x.Exchange,
                    x => x.Data.ToDictionary(y => y.Symbol.ToUpperInvariant()));
        }
    }
}
