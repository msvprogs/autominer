using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.HttpTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class QwakPoolInfoProvider : IPoolInfoProvider
    {
        private static readonly Dictionary<string, string> M_Headers =
            new Dictionary<string, string>
            {
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8"
            };

        private readonly IWebClient m_WebClient;
        private readonly string m_BaseUrl;
        private readonly int? m_UserId;
        private readonly string m_ApiKey;
        private readonly KnownCoinAlgorithm? m_CoinAlgorithm;

        public QwakPoolInfoProvider(
            IWebClient webClient, string baseUrl, int? userId, string apiKey, KnownCoinAlgorithm? coinAlgorithm)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiKey));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = baseUrl;
            m_UserId = userId;
            m_ApiKey = apiKey;
            m_CoinAlgorithm = coinAlgorithm;
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            double hashrate;
            double? poolHashrate = null;
            JObject shares;
            try
            {
                var userInfoJson = ExecuteApiMethod("getuserstatus");
                hashrate = userInfoJson.getuserstatus.data.hashrate;
                shares = userInfoJson.getuserstatus.data.shares;
            }
            catch (CorrectHttpException)
            {
                // For example, some SuprNova pools don't support getuserstatus method. Trying the other way...
                var dashboardData = ExecuteApiMethod("getdashboarddata");
                hashrate = dashboardData.getdashboarddata.data.raw.personal.hashrate;
                shares = dashboardData.getdashboarddata.data.personal.shares;
                poolHashrate = dashboardData.getdashboarddata.data.raw.pool.hashrate;
            }

            var balanceInfoJson = ExecuteApiMethod("getuserbalance");
            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = ((double?) balanceInfoJson.getuserbalance.data.confirmed).GetValueOrDefault(),
                UnconfirmedBalance = ((double?) balanceInfoJson.getuserbalance.data.unconfirmed).GetValueOrDefault(),
                HashRate = NormalizeHashRate(hashrate)
            };
            if (shares?.First != null)
            {
                accountInfo.ValidShares = (int) (shares["valid"]?.Value<double?>()).GetValueOrDefault();
                accountInfo.InvalidShares = (int) (shares["invalid"]?.Value<double?>()).GetValueOrDefault();
            }

            var state = new PoolState();
            try
            {
                var stateJson = ExecuteApiMethod("public");
                state.TotalHashRate = NormalizeHashRate((double) (stateJson.hashrate ?? stateJson.pool_hashrate));
                state.TotalWorkers = (int) (stateJson.workers ?? stateJson.pool_workers);
                if (stateJson.last_block != null)
                    state.LastBlock = (long) stateJson.last_block;
                
                var poolInfoJson = ExecuteApiMethod("getpoolinfo");
                if (poolInfoJson.getpoolinfo?.data?.fees != null)
                    state.PoolFee = (double) poolInfoJson.getpoolinfo.data.fees;
            }
            catch (CorrectHttpException)
            {
                // Some SuprNova pools don't support public methods either...
                state.TotalHashRate = NormalizeHashRate(poolHashrate);
                state.TotalWorkers = 0;
            }

            JArray paymentsArray;
            try
            {
                paymentsArray = (JArray) ExecuteApiMethod("getusertransactions")?.getusertransactions.data.transactions;
            }
            catch (CorrectHttpException)
            {
                paymentsArray = new JArray();
            }

            var payments = paymentsArray.EmptyIfNull()
                .Cast<dynamic>()
                .Where(x => x.amount != null)
                .Select(x => new
                {
                    Id = (string) x.id,
                    Type = (string) x.type,
                    Amount = (double) x.amount,
                    DateTime = (string) x.timestamp,
                    BlockHash = (string) x.blockhash,
                    TxHash = (string) x.txid,
                    Confirmations = (int?) x.confirmations,
                    Address = (string) x.coin_address
                })
                .Select(x => new PoolPaymentData
                {
                    Amount = x.Amount,
                    DateTime = DateTimeHelper.FromIso8601(x.DateTime),
                    Transaction = x.TxHash,
                    Address = x.Address,
                    BlockHash = x.BlockHash,
                    ExternalId = x.Id,
                    Type = GetPaymentType(x.Type)
                })
                .Where(x => x.DateTime > minPaymentDate)
                .ToArray();
            return new PoolInfo
            {
                AccountInfo = accountInfo,
                State = state,
                PaymentsData = payments
            };
        }

        private dynamic ExecuteApiMethod(string method)
        {
            var resultJson = m_WebClient.DownloadString(GetActionUri(method), M_Headers);
            return !string.IsNullOrWhiteSpace(resultJson)
                ? JsonConvert.DeserializeObject(resultJson)
                : null;
        }

        private long NormalizeHashRate(double? sourceHashRate)
        {
            if (sourceHashRate == null)
                return 0;
            var normalizedHashRate = m_CoinAlgorithm == KnownCoinAlgorithm.Equihash
                ? sourceHashRate / 1000
                : sourceHashRate * 1000;
            switch (new Uri(m_BaseUrl).Host.ToLowerInvariant())
            {
                case "btcz.suprnova.cc":
                    return (long) (normalizedHashRate * 1000);
                case "mnx.suprnova.cc":
                case "zero.suprnova.cc":
                    return (long) (normalizedHashRate / 1000);
                default:
                    return (long) normalizedHashRate;
            }
        }

        private string GetActionUri(string action)
        {
            var parameters = new Dictionary<string, string>
            {
                ["page"] = "api",
                ["action"] = action,
                ["api_key"] = m_ApiKey
            };
            if (m_UserId != null)
                parameters.Add("id", m_UserId.ToString());
            return m_BaseUrl + new QueryBuilder(parameters);
        }

        private static PoolPaymentType GetPaymentType(string type)
        {
            switch (type?.ToLowerInvariant())
            {
                case "fee":
                    return PoolPaymentType.PoolFee;
                case "txfee":
                    return PoolPaymentType.TransactionFee;
                case "credit":
                    return PoolPaymentType.Reward;
                case "debit_mp":
                case "debit_ap":
                    return PoolPaymentType.TransferToWallet;
                case "donation":
                    return PoolPaymentType.Donation;
                default:
                    return PoolPaymentType.Unknown;
            }
        }
    }
}
