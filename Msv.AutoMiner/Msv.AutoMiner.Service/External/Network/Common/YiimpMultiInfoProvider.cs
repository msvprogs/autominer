using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Infrastructure;
using NLog;

namespace Msv.AutoMiner.Service.External.Network.Common
{
    public class YiimpMultiInfoProvider : NetworkInfoProviderBase, IMultiCoinNetworkInfoProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("YiimpMultiInfoProvider");

        private readonly IDDoSTriggerPreventingDownloader m_Downloader;
        private readonly string m_BaseUrl;
        private readonly TimeZoneInfo m_ServerTimeZone;
        private readonly string[] m_CurrencySymbols;

        public YiimpMultiInfoProvider(
            IDDoSTriggerPreventingDownloader downloader, 
            string baseUrl,
            TimeZoneInfo serverTimeZone, 
            string[] currencySymbols)
        {
            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (serverTimeZone == null)
                throw new ArgumentNullException(nameof(serverTimeZone));
            if (currencySymbols == null)
                throw new ArgumentNullException(nameof(currencySymbols));

            m_Downloader = downloader;
            m_BaseUrl = baseUrl;
            m_ServerTimeZone = serverTimeZone;
            m_CurrencySymbols = currencySymbols;
        }

        public Dictionary<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_Downloader.DownloadString($"{m_BaseUrl}/explorer"));
            var hashRateNodes = mainPage.DocumentNode.SelectNodes("//tr[@class='ssrow']")
                .Select(x => new
                {
                    Currency = x.SelectSingleNode(".//td[3]")?.InnerText.Trim().ToUpperInvariant(),
                    HashRateNode = x.SelectSingleNode(".//td[9]")
                })
                .Where(x => x.Currency != null)
                .Where(x => m_CurrencySymbols.Contains(x.Currency))
                .ToDictionary(x => x.Currency, x => x.HashRateNode);

            return m_CurrencySymbols.Select(c =>
                {
                    try
                    {
                        return GetSingleStats(c, hashRateNodes);
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, $"Couldn't get network info for {c}");
                        return new KeyValuePair<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>>();
                    }
                })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private KeyValuePair<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>> GetSingleStats(
            string c, IReadOnlyDictionary<string, HtmlNode> hashRateNodes)
        {
            var transactionPage = new HtmlDocument();
            var transactionHtml = m_Downloader.DownloadString($"{m_BaseUrl}/explorer/{c}");
            transactionPage.LoadHtml(transactionHtml);
            var hasAlgorithm = transactionPage.DocumentNode.SelectSingleNode("//th[text()='Algo']") != null;
            var rows = transactionPage.DocumentNode.SelectNodes("//tr[@class='ssrow']")
                .Select(x => new
                {
                    DateTime = x.SelectSingleNode(".//td[1]/span").GetAttributeValue("title", string.Empty),
                    Height = long.Parse(x.SelectSingleNode(".//td[2]/a").InnerText),
                    Type = x.SelectSingleNode(".//td[4]").InnerText,
                    Difficulty = x.SelectSingleNode(".//td[3]").InnerText,
                    Algorithm = hasAlgorithm
                        ? x.SelectSingleNode(".//td[5]").InnerText.Trim()
                        : null
                })
                .Where(x => x.Type == "PoW" || x.Type == "Aux") //PoW = Proof-of-Work; Aux = Merged mining
                .ToArray();
            var lastHeight = rows.Max(x => x.Height);
            var blockPage = new HtmlDocument();
            var blockHtml = m_Downloader.DownloadString(
                $"{m_BaseUrl}/explorer/{c}?height={lastHeight}");
            blockPage.LoadHtml(blockHtml);

            var rewardElement = blockPage.DocumentNode.SelectSingleNode(
                "//td[text()='Generation']/preceding-sibling::td[1]");
            return new KeyValuePair<string, Dictionary<CoinAlgorithm, CoinNetworkStatistics>>(c,
                rows.GroupBy(x => GetAlgorithmFromString(x.Algorithm))
                    .ToDictionary(x => x.Key, a => new CoinNetworkStatistics
                    {
                        Difficulty =
                            ParsingHelper.ParseDouble(a.OrderByDescending(x => x.Height).First().Difficulty),
                        Height = lastHeight,
                        NetHashRate = ParsingHelper.ParseHashRate(
                            hashRateNodes[c].GetAttributeValue("data", hashRateNodes[c].InnerText)),
                        BlockTimeSeconds = CalculateBlockStats(a.Select(x => new BlockInfo
                        {
                            Height = x.Height,
                            Timestamp = TimestampHelper.ToTimestamp(
                                DateTime.ParseExact(x.DateTime, "yyyy-MM-dd HH:mm:ss",
                                    CultureInfo.InvariantCulture),
                                m_ServerTimeZone)
                        }))?.MeanBlockTime,
                        BlockReward = ParsingHelper.ParseDouble(rewardElement.InnerText)
                    }));
        }

        private static CoinAlgorithm GetAlgorithmFromString(string str)
        {
            switch (str)
            {
                case null:
                    return CoinAlgorithm.Unknown;
                case "blake2s":
                    return CoinAlgorithm.Blake2S;
                case "scrypt":
                    return CoinAlgorithm.Scrypt;
                case "x17":
                    return CoinAlgorithm.X17;
                case "myr-gr":
                    return CoinAlgorithm.MyriadGroestl;
                case "penta":
                    return CoinAlgorithm.Pentablake;
                case "whirlpool":
                    return CoinAlgorithm.Whirlpool;
                case "bastion":
                    return CoinAlgorithm.Bastion;
                case "x15":
                    return CoinAlgorithm.X15;
                case "lyra2v2":
                    return CoinAlgorithm.Lyra2REv2;
                default:
                    return CoinAlgorithm.Unknown;
            }
        }

        public override CoinNetworkStatistics GetNetworkStats()
        {
            throw new NotImplementedException();
        }
    }
}
