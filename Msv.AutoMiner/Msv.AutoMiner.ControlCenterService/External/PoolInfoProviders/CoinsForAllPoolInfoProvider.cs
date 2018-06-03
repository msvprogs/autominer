using System;
using System.Linq;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class CoinsForAllPoolInfoProvider : IPoolInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("http://coinsforall.io/api/");

        private readonly IWebClient m_WebClient;
        private readonly string m_CoinSymbol;
        private readonly string m_Wallet;

        public CoinsForAllPoolInfoProvider(IWebClient webClient, string coinSymbol, string wallet)
        {
            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_CoinSymbol = coinSymbol?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(coinSymbol));
            m_Wallet = wallet;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            dynamic poolStateJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"{m_CoinSymbol}/poolStats")));
            var poolState = new PoolState
            {
                TotalHashRate = (long) Math.Round((double) poolStateJson.power / 1000),
                TotalWorkers = (int) poolStateJson.workers
            };
            if (m_Wallet == null)
                return new PoolInfo
                {
                    AccountInfo = new PoolAccountInfo(),
                    PaymentsData = new PoolPaymentData[0],
                    State = poolState
                };

            dynamic accountInfoJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"{m_CoinSymbol}/clientInfo?userId={m_Wallet}")));
            dynamic clientStatsJson = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"{m_CoinSymbol}/clientStats?userId={m_Wallet}")));
            var paymentsJson = JsonConvert.DeserializeObject<JArray>(
                m_WebClient.DownloadString(new Uri(M_BaseUri, $"{m_CoinSymbol}/payouts?count=20&userId={m_Wallet}")));

            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = (double) accountInfoJson.balance / 1e8,
                HashRate = (long) Math.Round((double) clientStatsJson.total.power / 1000)
            };
            var payments = paymentsJson
                .Cast<dynamic>()
                .Select(x => new PoolPaymentData
                {
                    DateTime = DateTimeHelper.ToDateTimeUtc((long) x.time),
                    Transaction = (string) x.txid,
                    Amount = (double) x.value / 1e8,
                    Type = PoolPaymentType.Reward
                })
                .Where(x => x.DateTime > minPaymentDate)
                .ToArray();

            return new PoolInfo
            {
                AccountInfo = accountInfo,
                PaymentsData = payments,
                State = poolState
            };
        }
    }
}
