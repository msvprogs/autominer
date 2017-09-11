using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IMarketValuesProvider
    {
        Dictionary<ExchangeType, Dictionary<string, CoinMarketInfo>> GetCoinMarketValues(Coin[] coins);
    }
}
