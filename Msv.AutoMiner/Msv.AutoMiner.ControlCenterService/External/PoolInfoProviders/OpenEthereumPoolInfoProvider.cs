using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class OpenEthereumPoolInfoProvider : IPoolInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_Url;
        private readonly string m_Wallet;

        public OpenEthereumPoolInfoProvider(IWebClient webClient, string baseUrl, string wallet)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(wallet))
                throw new ArgumentException("Value cannot be null or empty.", nameof(wallet));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_Url = baseUrl;
            m_Wallet = wallet;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            dynamic accountJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(m_Url + "/accounts/" + m_Wallet));
            var accountInfo = new PoolAccountInfo
            {
                HashRate = (long) accountJson.currentHashrate,
                ValidShares = (int) (accountJson.roundShares / 1e9),
                ConfirmedBalance = (double) accountJson.stats.balance / 1e9,
                UnconfirmedBalance = (double) accountJson.stats.immature / 1e9
            };
            dynamic stateJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(m_Url + "/stats"));
            var state = new PoolState
            {
                TotalWorkers = (int) stateJson.minersTotal,
                LastBlock = (long) stateJson.stats.lastBlockFound,
                TotalHashRate = (long) stateJson.hashrate
            };
            var payments = accountJson.payments != null
                ? ((JArray) accountJson.payments)
                .Cast<dynamic>()
                .Select(x => new PoolPaymentData
                {
                    Amount = (double) x.amount / 1e9,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.timestamp),
                    Transaction = (string) x.tx
                })
                .Where(x => x.DateTime > minPaymentDate)
                .ToArray()
                : new PoolPaymentData[0];
            return new PoolInfo
            {
                AccountInfo = accountInfo,
                PaymentsData = payments,
                State = state
            };
        }
    }
}
