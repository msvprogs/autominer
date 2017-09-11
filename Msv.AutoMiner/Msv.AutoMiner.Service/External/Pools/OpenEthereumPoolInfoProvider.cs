using System;
using System.Linq;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Pools
{
    public class OpenEthereumPoolInfoProvider : WebDownloaderBase, IPoolInfoProvider
    {
        private readonly string m_Wallet;
        private readonly string m_Url;

        public OpenEthereumPoolInfoProvider(string baseUrl, string wallet)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(wallet))
                throw new ArgumentException("Value cannot be null or empty.", nameof(wallet));

            m_Url = baseUrl;
            m_Wallet = wallet;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            dynamic accountJson = JsonConvert.DeserializeObject(
                DownloadString(m_Url + "/accounts/" + m_Wallet));
            var accountInfo = new PoolAccountInfo
            {
                Hashrate = (long) accountJson.currentHashrate,
                ValidShares = (int) (accountJson.roundShares / 1e9),
                ConfirmedBalance = (double) accountJson.stats.balance / 1e9,
                UnconfirmedBalance = (double) accountJson.stats.immature / 1e9
            };
            dynamic stateJson = JsonConvert.DeserializeObject(DownloadString(m_Url + "/stats"));
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
                    DateTime = TimestampHelper.ToDateTime((long) x.timestamp),
                    Transaction = (string) x.tx
                })
                .Where(x => x.DateTime > minPaymentDate)
                .ToArray()
                : new PoolPaymentData[0];
            return new PoolInfo(accountInfo, state, payments);
        }
    }
}
