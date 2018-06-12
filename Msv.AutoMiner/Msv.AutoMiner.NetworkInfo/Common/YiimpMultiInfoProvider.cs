using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.NetworkInfo.Data;
using NLog;

namespace Msv.AutoMiner.NetworkInfo.Common
{
    public class YiimpMultiInfoProvider : NetworkInfoProviderBase, IMultiNetworkInfoProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IProxiedWebClient m_WebClient;
        private readonly string m_BaseUrl;
        private readonly TimeZoneInfo m_ServerTimeZone;
        private readonly string[] m_CurrencySymbols;

        public YiimpMultiInfoProvider(
            IProxiedWebClient webClient,
            string baseUrl,
            TimeZoneInfo serverTimeZone, 
            string[] currencySymbols)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = baseUrl;
            m_ServerTimeZone = serverTimeZone ?? throw new ArgumentNullException(nameof(serverTimeZone));
            m_CurrencySymbols = currencySymbols ?? throw new ArgumentNullException(nameof(currencySymbols));
        }

        public Dictionary<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetMultiNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_WebClient.DownloadString($"{m_BaseUrl}/explorer"));
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
                        return new KeyValuePair<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>();
                    }
                })
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private KeyValuePair<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>> GetSingleStats(
            string c, IReadOnlyDictionary<string, HtmlNode> hashRateNodes)
        {
            var transactionPage = new HtmlDocument();
            var transactionHtml = m_WebClient.DownloadStringProxied($"{m_BaseUrl}/explorer/{c}");
            transactionPage.LoadHtml(transactionHtml);
            var hasAlgorithm = transactionPage.DocumentNode.SelectSingleNode("//th[text()='Algo']") != null;
            var rows = transactionPage.DocumentNode.SelectNodes("//tr[@class='ssrow']")
                .Select(x => new
                {
                    DateTime = x.SelectSingleNode(".//td[1]/span").GetAttributeValue("title", string.Empty),
                    Height = long.Parse(x.SelectSingleNode(".//td[2]").InnerText),
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
            var blockHtml = m_WebClient.DownloadStringProxied(
                $"{m_BaseUrl}/explorer/{c}?height={lastHeight}");
            blockPage.LoadHtml(blockHtml);

            var rewardElement = blockPage.DocumentNode.SelectSingleNode(
                "//td[text()='Generation']/preceding-sibling::td[1]");
            return new KeyValuePair<string, Dictionary<KnownCoinAlgorithm, CoinNetworkStatistics>>(c,
                rows.GroupBy(x => GetAlgorithmFromString(x.Algorithm))
                    .ToDictionary(x => x.Key, a => new CoinNetworkStatistics
                    {
                        Difficulty =
                            ParsingHelper.ParseDouble(a.OrderByDescending(x => x.Height).First().Difficulty),
                        Height = lastHeight,
                        NetHashRate = ParsingHelper.ParseHashRate(
                            hashRateNodes[c].GetAttributeValue("data", hashRateNodes[c].InnerText)),
                        BlockTimeSeconds = CalculateBlockStats(a.Select(x => new BlockInfo(
                            DateTimeHelper.TimestampFromIso8601(x.DateTime, m_ServerTimeZone),
                            x.Height)))?.MeanBlockTime,
                        BlockReward = ParsingHelper.ParseDouble(rewardElement.InnerText)
                    }));
        }

        private static KnownCoinAlgorithm GetAlgorithmFromString(string str)
        {
            switch (str)
            {
                case "blake2s":
                    return KnownCoinAlgorithm.Blake2S;
                case "scrypt":
                    return KnownCoinAlgorithm.Scrypt;
                case "x17":
                    return KnownCoinAlgorithm.X17;
                case "myr-gr":
                    return KnownCoinAlgorithm.MyriadGroestl;
                case "penta":
                    return KnownCoinAlgorithm.Pentablake;
                case "whirlpool":
                    return KnownCoinAlgorithm.Whirlpool;
                case "bastion":
                    return KnownCoinAlgorithm.Bastion;
                case "x15":
                    return KnownCoinAlgorithm.X15;
                case "lyra2v2":
                    return KnownCoinAlgorithm.Lyra2Rev2;
                default:
                    return KnownCoinAlgorithm.Unknown;
            }
        }

        public override CoinNetworkStatistics GetNetworkStats() 
            => throw new InvalidOperationException("Use GetMultiNetworkStats() method");

        public override WalletBalance GetWalletBalance(string address)
        {
            throw new NotImplementedException();
        }

        public override BlockExplorerWalletOperation[] GetWalletOperations(string address, DateTime startDate)
        {
            throw new NotImplementedException();
        }

        public override Uri CreateTransactionUrl(string hash)
            => null;

        public override Uri CreateAddressUrl(string address)
            => null;

        public override Uri CreateBlockUrl(string blockHash)
            => null;
    }
}
