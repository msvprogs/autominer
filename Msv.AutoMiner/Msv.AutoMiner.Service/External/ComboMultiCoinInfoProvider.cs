using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;

namespace Msv.AutoMiner.Service.External
{
    public class ComboMultiCoinInfoProvider : IMultiCoinNetworkInfoProvider
    {
        private readonly IMultiCoinNetworkInfoProvider[] m_Providers;

        public ComboMultiCoinInfoProvider(params IMultiCoinNetworkInfoProvider[] providers)
        {
            if (providers == null)
                throw new ArgumentNullException(nameof(providers));
            m_Providers = providers;
        }

        public Dictionary<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
            => m_Providers.SelectMany(x => x.GetMultiNetworkStats())
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Value)
                    .GroupBy(y => y.Key)
                    .ToDictionary(y => y.Key, y => y.First().Value));
    }
}
