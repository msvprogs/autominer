using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface ICoinMarketInfoAggregator
    {
        Dictionary<string, double> GetAggregatedMarketPrices(
            Dictionary<ExchangeType, Dictionary<string, CoinMarketInfo>> currentMarketValues);
    }
}
