﻿using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://www.etherchain.org/documentation/api
    public class EthereumInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_EtherchainBaseUri = new Uri("https://www.etherchain.org");

        private readonly IWebClient m_WebClient;

        public EthereumInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic statsJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/miningEstimator")));
            dynamic heightJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_EtherchainBaseUri, "/api/blocks/count")));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) statsJson.difficulty,
                NetHashRate = (double) statsJson.hashrate,
                BlockTimeSeconds = (double) statsJson.blocktime,
                Height = (long) heightJson.count
                //TODO: add last block time (now API documentation is unavailable)
            };
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_EtherchainBaseUri, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_EtherchainBaseUri, $"account/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_EtherchainBaseUri, $"block/{blockHash}");
    }
}