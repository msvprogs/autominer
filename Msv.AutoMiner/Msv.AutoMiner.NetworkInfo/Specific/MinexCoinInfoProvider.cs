﻿using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("MNX")]
    public class MinexCoinInfoProvider : INetworkInfoProvider
    {
        // https://minexexplorer.com
        private static readonly Uri M_BaseUri = new Uri("http://0s.nvuw4zlymv4ha3dpojsxeltdn5wq.cmle.ru/");
        private static readonly TimeZoneInfo M_ServerTimeZone =
            TimeZoneInfo.CreateCustomTimeZone("GMT+3", TimeSpan.FromHours(3), "GMT+3", "GMT+3");

        private readonly IWebClient m_WebClient;

        public MinexCoinInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public CoinNetworkStatistics GetNetworkStats()
        {
            var mainPage = new HtmlDocument();
            mainPage.LoadHtml(m_WebClient.DownloadString(M_BaseUri));

            var lastBlockRow = mainPage.DocumentNode.SelectSingleNode("//table[@class='table main-table']//tr[2]");
            var hashrate = JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "hashrate.json")))
                .Last();
            var difficulty = JsonConvert.DeserializeObject<JArray>(
                    m_WebClient.DownloadString(new Uri(M_BaseUri, "difficulty.json")))
                .Last();

            return new CoinNetworkStatistics
            {
                Height = long.Parse(lastBlockRow.SelectSingleNode("./td[1]/a").InnerText),
                Difficulty = ((JArray) difficulty).Last.Value<double>(),
                NetHashRate = ((JArray) hashrate).Last.Value<double>(),
                LastBlockTime = TimeZoneInfo.ConvertTimeToUtc(
                    DateTime.ParseExact(
                        lastBlockRow.SelectSingleNode("./td[2]").InnerText,
                        "dd.MM.yy HH:mm:ss",
                        CultureInfo.InvariantCulture),
                    M_ServerTimeZone)
            };
        }

        public Uri CreateTransactionUrl(string hash)
            => new Uri(M_BaseUri, $"?r=explorer/tx&hash={hash}");

        public Uri CreateAddressUrl(string address)
            => new Uri(M_BaseUri, $"?r=explorer/address&hash={address}");

        public Uri CreateBlockUrl(string blockHash)
            => new Uri(M_BaseUri, $"?r=explorer/block&hash={blockHash}");
    }
}