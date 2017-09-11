using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    public class Coimatic2InfoProvider : IquidusWithNonNumericDifficultyInfoProvider
    {
        public Coimatic2InfoProvider()
            : base("http://195.181.247.196:3001/")
        { }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            try
            {
                return base.GetNetworkStats();
            }
            catch
            {
                return GetAlternativeNetworkStats();
            }
        }

        private CoinNetworkStatistics GetAlternativeNetworkStats()
        {
            var html = new HtmlDocument();
            html.LoadHtml(DownloadString("http://198.199.90.93/Coimatic2/index.php"));
            return new CoinNetworkStatistics
            {
                Height = long.Parse(html.DocumentNode
                    .SelectSingleNode("//td[contains(.,'Blocks:')]/following-sibling::td/a").InnerText),
                Difficulty = ParsingHelper.ParseDouble(
                    html.DocumentNode.SelectSingleNode("//td[contains(.,'Difficulty')]/following-sibling::td").InnerText
                        .Split()[0]),
                NetHashRate = ParsingHelper.ParseHashRate(
                    html.DocumentNode.SelectSingleNode("//td[contains(.,'Network Hashrate:')]/following-sibling::td")
                        .InnerText)
            };
        }
    }
}
