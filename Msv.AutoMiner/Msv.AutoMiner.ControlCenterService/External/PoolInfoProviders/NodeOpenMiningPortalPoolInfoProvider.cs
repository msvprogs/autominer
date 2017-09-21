using System;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class NodeOpenMiningPortalPoolInfoProvider : IPoolInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_Url;
        private readonly string m_Wallet;
        private readonly string m_PoolName;

        public NodeOpenMiningPortalPoolInfoProvider(
            IWebClient webClient, string url, string wallet, string poolName)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("Value cannot be null or empty.", nameof(url));
            if (string.IsNullOrEmpty(wallet))
                throw new ArgumentException("Value cannot be null or empty.", nameof(wallet));
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(poolName));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_Url = url;
            m_Wallet = wallet;
            m_PoolName = poolName;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            dynamic stats = JsonConvert.DeserializeObject(m_WebClient.DownloadString(m_Url + "/stats"));
            var allBalances = stats.pools[m_PoolName].allbalances;
            var balanceItem = allBalances?[m_Wallet];
            var payouts = stats.pools[m_PoolName].payouts;
            var payout = payouts != null ? ((JToken)payouts)[m_Wallet]?.Value<double>() : null;
            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = balanceItem != null
                    ? (double) balanceItem.balance
                    : payout.GetValueOrDefault(),
                UnconfirmedBalance = balanceItem != null
                    ? (double) balanceItem.immature
                    : 0
            };
            dynamic worker = ((JToken)stats.pools[m_PoolName].workers)[m_Wallet];
            if (worker != null)
            {
                accountInfo.HashRate = ParsingHelper.ParseHashRate((string) worker.hashrateString);
                accountInfo.ValidShares = (int) (double) worker.shares;
                accountInfo.InvalidShares = (int) (double) worker.invalidshares;
            }
            var algorithmString = (string) stats.pools[m_PoolName].algorithm;
            var state = new PoolState
            {
                TotalHashRate = ParsingHelper.ParseHashRate(
                    (string) stats.algos[algorithmString].hashrateString),
                TotalWorkers = (int) stats.algos[algorithmString].workers
            };
            return new PoolInfo
            {
                AccountInfo = accountInfo,
                State = state,
                PaymentsData = new PoolPaymentData[0]
            };
        }
    }
}
