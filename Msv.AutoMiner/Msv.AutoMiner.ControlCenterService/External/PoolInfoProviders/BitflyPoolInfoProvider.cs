using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class BitflyPoolInfoProvider : IPoolInfoProvider
    {
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
            dynamic accountResult = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_Url, $"/miner/{m_Wallet}/dashboard").ToString()));
            var accountData = accountResult.data.currentStatistics;
            var accountInfo = new PoolAccountInfo
                {
                    HashRate = (long?)(double?)accountData.currentHashrate ?? 0,
                    ValidShares = (int?)accountData.validShares ?? 0,
                    InvalidShares = (int?)accountData.invalidShares + (int?)accountData.staleShares ?? 0,
                    ConfirmedBalance = (double?) accountData.unpaid / 1e8 ?? 0,
                    UnconfirmedBalance = (double?) accountData.unconfirmed / 1e8 ?? 0
                };

            dynamic stateJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_Url, "/poolStats").ToString()));
            var stateInfo = new PoolState
            {
                LastBlock = (long) stateJson.data.minedBlocks[0].number,
                TotalHashRate = (long) stateJson.data.poolStats.hashRate,
                TotalWorkers = (int) stateJson.data.poolStats.workers
            };

            dynamic payoutsJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                new Uri(m_Url, $"/miner/{m_Wallet}/payouts").ToString()));
            var payments = ((JArray) payoutsJson.data)
                .Cast<dynamic>()
                .Select(x => new PoolPaymentData
                {
                    Amount = (double) x.amount / 1e8,
                    Transaction = (string) x.txHash,
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.paidOn),
                    Type = PoolPaymentType.Reward
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
