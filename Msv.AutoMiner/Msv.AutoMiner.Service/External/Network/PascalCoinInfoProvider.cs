using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Network
{
    //API: http://explorer.pascalcoin.org/api.php
    public class PascalCoinInfoProvider : NetworkInfoProviderBase
    {
        public override CoinNetworkStatistics GetNetworkStats()
        {
            var height = long.Parse(DownloadString("http://explorer.pascalcoin.org/api.php?blockcount"));
            var blocks = Enumerable.Range(1, 5)
                .Select(x => DownloadString($"http://explorer.pascalcoin.org/api.php?block={height - x}"))
                .Select(JsonConvert.DeserializeObject)
                .Cast<dynamic>()
                .Select(x => new BlockInfo
                {
                    Height = (long) x.block,
                    Reward = (double) x.reward,
                    Timestamp = (long) x.timestamp
                })
                .ToArray();
            var hashrate = long.Parse(DownloadString("http://explorer.pascalcoin.org/api.php?hashrate")) * 1000;
            var blockStats = CalculateBlockStats(blocks);
            if (blockStats == null)
                return new CoinNetworkStatistics {Height = height, NetHashRate = hashrate};
            return new CoinNetworkStatistics
            {
                BlockReward = blockStats.Value.LastReward,
                BlockTimeSeconds = blockStats.Value.MeanBlockTime,
                Height = height,
                NetHashRate = hashrate
            };
        }
    }
}
