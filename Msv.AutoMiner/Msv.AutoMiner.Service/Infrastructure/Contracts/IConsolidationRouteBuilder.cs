using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IConsolidationRouteBuilder
    {
        Dictionary<ExchangeType, Dictionary<ExchangeType, ConsolidationRoute[]>> BuildForBtc(
            Dictionary<ExchangeType, Dictionary<string, CoinMarketInfo>> marketValues);
    }
}
