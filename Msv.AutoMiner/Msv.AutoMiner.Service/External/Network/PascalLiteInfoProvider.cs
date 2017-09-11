using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebSocketSharp;

namespace Msv.AutoMiner.Service.External.Network
{
    public class PascalLiteInfoProvider : NetworkInfoProviderBase
    {
        public override CoinNetworkStatistics GetNetworkStats()
        {
            using (var webSocket = new WebSocket("wss://explorer.pascallite.com/websocket"))
            {
                dynamic response = JsonConvert.DeserializeObject(
                    ExecuteWebSocketRequest(webSocket, JsonConvert.SerializeObject(new {method = "networkStats"})));
                var blockStats = CalculateBlockStats(((JArray) response.lastBlocks)
                    .Cast<dynamic>()
                    .Select(x => new BlockInfo
                    {
                        Height = (long) x.block,
                        Reward = (double) x.reward,
                        Timestamp = (long) x.timestamp
                    }));
                if (blockStats == null)
                    return new CoinNetworkStatistics();
                var difficultyPage = new HtmlDocument();
                difficultyPage.LoadHtml(DownloadString("https://pasl.suprnova.cc/index.php?page=statistics&action=pool"));

                return new CoinNetworkStatistics
                {
                    BlockReward = blockStats.Value.LastReward,
                    BlockTimeSeconds = blockStats.Value.MeanBlockTime,
                    Height = blockStats.Value.Height,
                    NetHashRate = (long) response.networkHashrate,
                    Difficulty = ParsingHelper.ParseDouble(
                        difficultyPage.DocumentNode.SelectSingleNode("//span[@id='b-diff']").InnerText)
                };
            }
        }
    }
}
