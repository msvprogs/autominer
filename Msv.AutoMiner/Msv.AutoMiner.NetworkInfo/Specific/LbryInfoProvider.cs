using System;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Common;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;

// ReSharper disable ArgumentsStyleLiteral

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("LBC")]
    public class LbryInfoProvider : NetworkInfoProviderBase
    {
        private const long StartingSubsidy = 500;
        private const long SubsidyLevelInterval = 1 << 5;
        private static readonly Uri M_BaseUri = new Uri("https://explorer.lbry.io");

        private readonly IWebClient m_WebClient;

        public LbryInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/v1/status")));
            if (!(bool) stats.success)
                throw new ExternalDataUnavailableException();

            dynamic recentBlocks = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, "/api/v1/recentblocks")));

            var height = (long) stats.status.height;
            return new CoinNetworkStatistics
            {
                Difficulty = ParsingHelper.ParseDouble((string)stats.status.difficulty),
                Height = height,
                BlockReward = CalculateBlockReward(height),
                NetHashRate = ParsingHelper.ParseHashRate((string)stats.status.hashrate),
                LastBlockTime = DateTimeHelper.ToDateTimeUtc((long)recentBlocks.blocks[0].BlockTime)
            };
        }

        public override Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"tx/{hash}");

        public override Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"address/{address}");

        public override Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"block/{blockHash}");

        //Source:
        //https://github.com/lbryio/lbrycrd/blob/master/src/main.cpp
        //https://github.com/lbryio/lbrycrd/blob/master/src/chainparams.cpp 
        private static double CalculateBlockReward(long height)
        {
            if (height == 0)
                return 400000000;
            if (height <= 5100)
                return 1;
            if (height <= 55000)
                return Math.Ceiling((height - 5000) / 100.0);
            var level = (height - 55001) / SubsidyLevelInterval;
            var reduction = -1 + (long)Math.Sqrt(8 * level + 1) / 2;
            while (!WithinLevelBounds(reduction, level))
            {
                if ((reduction * reduction + reduction) >> 1 > level)
                    reduction--;
                else
                    reduction++;
            }
            return Math.Max(0, StartingSubsidy - reduction);
        }

        private static bool WithinLevelBounds(long reduction, long level)
        {
            if ((reduction * reduction + reduction) >> 1 > level)
                return false;
            reduction++;
            return (reduction * reduction + reduction) >> 1 > level;
        }
    }
}
