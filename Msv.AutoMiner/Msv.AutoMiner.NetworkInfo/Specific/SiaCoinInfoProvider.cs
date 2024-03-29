﻿using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: https://siamining.com/api/
    [SpecificCoinInfoProvider("SC")]
    public class SiaCoinInfoProvider : INetworkInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public SiaCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic json = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://siamining.com/api/v1/network"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) json.difficulty,
                NetHashRate = (long) json.hash_rate,
                BlockReward = (double) json.block_reward,
                Height = (long) json.block_height
                // TODO: last block time
            };
        }

        public WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri("https://explore.sia.tech/hashes/" + hash);

        public Uri CreateAddressUrl(string address)
            => null;

        public Uri CreateBlockUrl(string blockHash)
            => CreateTransactionUrl(blockHash);
    }
}
