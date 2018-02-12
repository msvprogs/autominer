using System;
using System.Globalization;
using System.IO;
using System.Linq;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class BitflyPoolInfoProvider : IPoolInfoProvider
    {
        private static readonly string[] M_DateSplitter = {"GMT"};
        private static readonly TimeZoneInfo M_CestTimeZone = TimeZoneInfo.CreateCustomTimeZone(
            "CEST", TimeSpan.FromHours(2), "CEST", "CEST");

        private readonly IWebClient m_WebClient;
        private readonly string m_Wallet;
        private readonly Uri m_Url;

        public BitflyPoolInfoProvider(IWebClient webClient, string baseUrl, string wallet)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(wallet))
                throw new ArgumentException("Value cannot be null or empty.", nameof(wallet));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_Url = new Uri(baseUrl);
            m_Wallet = wallet;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            dynamic accountJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_Url, Path.Combine("/api/miner_new/", m_Wallet)).ToString()));
            var accountInfo = accountJson.hashRate != null
                ? new PoolAccountInfo
                {
                    HashRate = (long)ParsingHelper.ParseHashRate((string) accountJson.hashRate),
                    ConfirmedBalance = (double) accountJson.unpaid / 1e8
                }
                : new PoolAccountInfo();
            dynamic stateJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_Url, "/api/basic_stats").ToString()));
            var stateInfo = new PoolState
            {
                LastBlock = (long) stateJson.data.block,
                TotalHashRate = (long)ParsingHelper.ParseHashRate((string) stateJson.data.hashRate),
                TotalWorkers = (int) stateJson.data.minerCount
            };
            var page = new HtmlDocument();
            page.LoadHtml(m_WebClient.DownloadString(new Uri(m_Url, $"/miners/{m_Wallet}/payouts").ToString()));
            var payments = page
                .DocumentNode.SelectNodes("//tr[@id='payouts']")
                ?.Select(x => new PoolPaymentData
                {
                    DateTime = DateTimeHelper.Normalize(
                        TimeZoneInfo.ConvertTime(
                            DateTime.Parse(x.SelectSingleNode(".//td[1]").InnerText
                                    .Split(M_DateSplitter, StringSplitOptions.None)[0],
                                CultureInfo.InvariantCulture),
                            M_CestTimeZone,
                            TimeZoneInfo.Utc)),
                    Amount = ParsingHelper.ParseDouble(
                        x.SelectSingleNode(".//td[5]").InnerText.Split()[0]),
                    Transaction = new Uri(x.SelectSingleNode(".//td[6]/a")
                        .GetAttributeValue("href", null)).Segments.Last()
                })
                .Where(x => x.DateTime > minPaymentDate)
                .ToArray();
            return new PoolInfo
            {
                AccountInfo = accountInfo,
                PaymentsData = payments.EmptyIfNull(),
                State = stateInfo
            };
        }
    }
}
