using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.NetworkInfo.Data;
using NLog;

namespace Msv.AutoMiner.NetworkInfo
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

        public Uri CreateTransactionUrl(string hash)
            => m_Providers.FirstOrDefault()?.CreateTransactionUrl(hash);

        public Uri CreateAddressUrl(string address)
            => m_Providers.FirstOrDefault()?.CreateAddressUrl(address);

        public Uri CreateBlockUrl(string blockHash)
            => m_Providers.FirstOrDefault()?.CreateBlockUrl(blockHash);
    }
}