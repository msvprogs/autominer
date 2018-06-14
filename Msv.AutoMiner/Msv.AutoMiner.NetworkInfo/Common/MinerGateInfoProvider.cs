using System;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    //MinerGate API: https://github.com/MinerGate/minergate-api
    public class MinerGateInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_ApiBaseUri = new Uri("https://api.minergate.com");
        private static readonly Uri M_ExplorerBaseUri = new Uri("https://minergate.com/blockchain/");

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencySymbol;

        public MinerGateInfoProvider(IWebClient webClient, string currencySymbol)
        {
            if (string.IsNullOrEmpty(currencySymbol))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencySymbol));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencySymbol = currencySymbol.ToLowerInvariant();
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            var json = m_WebClient.DownloadJsonAsDynamic(
                new Uri(M_ApiBaseUri, $"/1.0/{m_CurrencySymbol}/status"));
            return new CoinNetworkStatistics
            {
                BlockReward = (double) json.reward,
                Difficulty = (double) json.difficulty,
                NetHashRate = (long) json.instantHashrate,
                Height = (long) json.height,
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long) json.timestamp)
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
            => new Uri(CreateCurrencyBaseUrl(), $"transaction/{hash}");

        public Uri CreateAddressUrl(string address)
            => null;

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(CreateCurrencyBaseUrl(), $"block/{blockHash}");

        private Uri CreateCurrencyBaseUrl()
            => new Uri(M_ExplorerBaseUri, $"{m_CurrencySymbol}/");
    }
}