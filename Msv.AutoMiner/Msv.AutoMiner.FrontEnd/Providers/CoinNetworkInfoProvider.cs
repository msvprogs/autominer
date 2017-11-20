using System.Linq;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class CoinNetworkInfoProvider : ICoinNetworkInfoProvider
    {
        private readonly AutoMinerDbContext m_Context;

        public CoinNetworkInfoProvider(AutoMinerDbContext context)
            => m_Context = context;

        public CoinNetworkInfo[] GetCurrentNetworkInfos()
            => m_Context.CoinNetworkInfos
                .GroupBy(x => x.CoinId)
                .Select(x => x.OrderByDescending(y => y.Created).FirstOrDefault())
                .ToArray();
    }
}
