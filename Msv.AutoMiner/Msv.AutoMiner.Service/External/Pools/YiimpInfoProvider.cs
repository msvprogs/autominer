using System;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Service.External.Pools
{
    public class YiimpInfoProvider : IPoolInfoProvider
    {
        private readonly IDDoSTriggerPreventingDownloader m_Downloader;
        private readonly string m_BaseUrl;
        private readonly string m_Wallet;
        private readonly string m_PoolName;

        public YiimpInfoProvider(IDDoSTriggerPreventingDownloader downloader, string baseUrl, string wallet, string poolName)
        {
            if (downloader == null)
                throw new ArgumentNullException(nameof(downloader));
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(wallet))
                throw new ArgumentException("Value cannot be null or empty.", nameof(wallet));
            if (string.IsNullOrEmpty(poolName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(poolName));

            m_Downloader = downloader;
            m_BaseUrl = baseUrl;
            m_Wallet = wallet;
            m_PoolName = poolName;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            var workerInfoString = m_Downloader.DownloadString($"{m_BaseUrl}/walletEx?address={m_Wallet}");
            var accountInfo = new PoolAccountInfo();
            if (!string.IsNullOrWhiteSpace(workerInfoString))
            {
                dynamic workerJson = JsonConvert.DeserializeObject(workerInfoString);
                accountInfo.ConfirmedBalance = (double)workerJson.balance;
                accountInfo.UnconfirmedBalance = ((double?)workerJson.unsold).GetValueOrDefault();
                var miner = workerJson.miners.Count > 0 ? workerJson.miners[0] : null;
                if (miner != null)
                {
                    accountInfo.ValidShares = (int)(double)miner.accepted;
                    accountInfo.InvalidShares = (int)(double)miner.rejected;
                }
            }
            dynamic poolsJson = JsonConvert.DeserializeObject(m_Downloader.DownloadString($"{m_BaseUrl}/status"));
            var poolInfo = poolsJson[m_PoolName];
            var poolState = new PoolState
            {
                TotalWorkers = (int) poolInfo.workers,
                TotalHashRate = (long) poolInfo.hashrate
            };
            return new PoolInfo(accountInfo, poolState, new PoolPaymentData[0]);
        }
    }
}
