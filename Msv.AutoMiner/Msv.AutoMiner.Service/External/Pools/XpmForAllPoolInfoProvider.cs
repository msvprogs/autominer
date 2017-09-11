using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using HtmlAgilityPack;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;

namespace Msv.AutoMiner.Service.External.Pools
{
    //not finished
    public class XpmForAllPoolInfoProvider : WebDownloaderBase, IPoolInfoProvider
    {
        private static readonly Dictionary<HttpRequestHeader, string> M_Headers =
            new Dictionary<HttpRequestHeader, string>
            {
                [HttpRequestHeader.UserAgent] = UserAgent,
                [HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                [HttpRequestHeader.AcceptLanguage] = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3"
            };

        private readonly Uri m_BaseUrl;
        private readonly string m_Wallet;

        public XpmForAllPoolInfoProvider(string baseUrl, string wallet)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(wallet))
                throw new ArgumentException("Value cannot be null or empty.", nameof(wallet));

            m_BaseUrl = new Uri(baseUrl);
            m_Wallet = wallet;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            var firstPage = new HtmlDocument();
            firstPage.LoadHtml(DownloadString(m_BaseUrl.ToString(), M_Headers));
            var noJsPostfix = firstPage.DocumentNode.SelectSingleNode("//meta[@http-equiv='refresh']")
                .GetAttributeValue("content", string.Empty)
                .Split(new[] {'='}, 2)
                .Last();
            Thread.Sleep(1000);
            var mainPage = new HtmlDocument();
            var mainPageHtml = DownloadString(m_BaseUrl + noJsPostfix, M_Headers);
            mainPage.LoadHtml(mainPageHtml);
            var inputs = mainPage.DocumentNode.SelectNodes("//input")
                .Select(x => new
                {
                    Name = x.GetAttributeValue("name", string.Empty),
                    Value = x.GetAttributeValue("value", string.Empty)
                });
            var postContent = new MultipartFormDataContent($"-----------------------------{DateTime.Now.Ticks / 100000}");
            foreach (var input in inputs)
                postContent.Add(new StringContent(input.Value), input.Name);
            var postRequest = postContent.ReadAsStringAsync().Result;
            var postResult = UploadString(m_BaseUrl + noJsPostfix, postRequest,
                new Dictionary<string, string>
                {
                    ["Cookie"] = $"address={m_Wallet}",
                    ["User-Agent"] = UserAgent
                });
            var resultPage = new HtmlDocument();
            resultPage.LoadHtml(postResult);
            var workers = resultPage.DocumentNode
                .SelectSingleNode("//legend[text()='Workers']/following-sibling::span").InnerText;
            var totalCpd = resultPage.DocumentNode
                .SelectSingleNode("//legend[text()='CPD']/following-sibling::span").InnerText;
            var balance = resultPage.DocumentNode
                .SelectSingleNode("//tbody/tr/td[1]/button/div[contains(text(),'XPT')]").InnerText.Trim().Split()[0];
            var chainsRate = resultPage.DocumentNode
                .SelectSingleNode("//tbody/tr/td[2]/button/div[contains(text(),'chains/day')]").InnerText;
            return new PoolInfo(
                new PoolAccountInfo
                {
                    ConfirmedBalance = ParsingHelper.ParseDouble(balance),
                    Hashrate = ParsingHelper.ParseHashRate(chainsRate)
                },
                new PoolState
                {
                    TotalWorkers = int.Parse(workers),
                    TotalHashRate = ParsingHelper.ParseHashRate(totalCpd)
                },
                new PoolPaymentData[0]);       
        }
    }
}
