using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.Enums;
using NLog;

namespace Msv.AutoMiner.CoinInfoService.External
{
    public class ComboMultiNetworkInfoProvider : IMultiNetworkInfoProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IMultiNetworkInfoProvider[] m_Providers;

        public ComboMultiNetworkInfoProvider(params IMultiNetworkInfoProvider[] providers) 
            => m_Providers = providers ?? throw new ArgumentNullException(nameof(providers));

        public Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
            => m_Providers.SelectMany(x =>
                {
                    try
                    {
                        return x.GetMultiNetworkStats();
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, "Multinetwork provider error");
                        return new Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>();
                    }
                })
                .GroupBy(x => x.Key)
                .ToDictionary(x => x.Key, x => x.SelectMany(y => y.Value)
                    .GroupBy(y => y.Key)
                    .ToDictionary(y => y.Key, y => y.First().Value));
    }
}