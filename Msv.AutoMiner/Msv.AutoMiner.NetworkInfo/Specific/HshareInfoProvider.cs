using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    //API: http://explorer.h.cash/api
    [SpecificCoinInfoProvider("HSR")]
    public class HshareInfoProvider : INetworkInfoProvider
    {
        private const long StakeMinConfirmations = 50;
        private const double GlobalMoney = 84000000;
        private const long V1StartBlock = 106000;
        private const long PoWEndBlock = 52560000;

        private static readonly Uri M_BaseUri = new Uri("http://explorer.h.cash");

        private readonly IWebClient m_WebClient;

        public HshareInfoProvider(IWebClient webClient) 
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            var mainInfo = JsonConvert.DeserializeObject<dynamic>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/ajax").ToString(),
                    new Dictionary<string, string>
                    {
                        ["Referer"] = M_BaseUri.ToString()
                    }));

            var height = (long) mainInfo.block_number;
            var bestBlockHash = m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/getblockhash/" + height))
                .Trim('"');
            dynamic bestBlockInfo = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/getblock/" + bestBlockHash)));

            return new CoinNetworkStatistics
            {
                Height = height,
                BlockReward = GetBlockReward(height + 1),
                Difficulty = (double) mainInfo.difficulty["proof-of-work"],
                NetHashRate = ParsingHelper.ParseHashRate(
                    string.Join(" ", (string) mainInfo.rate, (string) mainInfo.rate_unit)),
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)bestBlockInfo.time)
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
            => new Uri(M_BaseUri, $"tx/{hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"address/{address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");

        private static double GetBlockReward(long height)
        {
            if (height < StakeMinConfirmations)
                return GlobalMoney / StakeMinConfirmations;
            if (height < V1StartBlock)
                return 0;
            if (height < PoWEndBlock)
                return 1.6 - 0.016 * (height / 525600.0);
            return 0;
        }
    }
}
