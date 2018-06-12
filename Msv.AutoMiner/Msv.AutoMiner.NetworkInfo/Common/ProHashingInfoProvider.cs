using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class ProHashingInfoProvider : INetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://prohashing.com");

        private readonly IWebClient m_WebClient;
        private readonly string m_CurrencyName;

        public ProHashingInfoProvider(IWebClient webClient, string currencyName)
        {
            if (string.IsNullOrEmpty(currencyName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(currencyName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CurrencyName = currencyName;
        }

        public CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"/explorerJson/getInfo?coin_name={m_CurrencyName}")));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) stats.difficulty,
                Height = (long) stats.blocks,
                LastBlockTime = DateTimeHelper.ToDateTimeUtcMsec((long)stats.last_indexed_block_time)
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
            => null;

        public Uri CreateAddressUrl(string address)
            => null;

        public Uri CreateBlockUrl(string blockHash)
            => null;
    }
}
