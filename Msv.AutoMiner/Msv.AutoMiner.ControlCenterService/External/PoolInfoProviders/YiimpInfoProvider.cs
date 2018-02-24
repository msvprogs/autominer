using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Data;
using Msv.HttpTools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class YiimpInfoProvider : IMultiPoolInfoProvider
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IProxiedWebClient m_WebClient;
        private readonly string m_ApiUrl;
        private readonly Pool[] m_Pools;
        private readonly string m_PoolUrl;
        private readonly Wallet m_BtcMiningTarget;

        public YiimpInfoProvider(IProxiedWebClient webClient, string apiUrl, Pool[] pools, Wallet btcMiningTarget)
        {
            if (string.IsNullOrEmpty(apiUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(apiUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_ApiUrl = apiUrl;
            m_Pools = pools ?? throw new ArgumentNullException(nameof(pools));
            m_PoolUrl = pools.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.ApiSecondaryUrl))?.ApiSecondaryUrl;
            m_BtcMiningTarget = btcMiningTarget ?? throw new ArgumentNullException(nameof(btcMiningTarget));
        }

        public MultiPoolInfo GetInfo(DateTime minPaymentDate)
        {
            var currenciesJson = DownloadDirectlyOrViaProxy($"{m_ApiUrl}/currencies");
            if (string.IsNullOrWhiteSpace(currenciesJson))
                throw new ExternalDataUnavailableException("Currencies method returned an empty result");
            var currenciesJObject = JsonConvert.DeserializeObject<JObject>(currenciesJson);
            var currencies = currenciesJObject
                .Properties()
                .Cast<dynamic>()
                .Select(x => new PoolCurrencyInfo
                {
                    Symbol = (string) x.Name,
                    Algorithm = (string) x.Value.algo,
                    Hashrate = (double?) x.Value.hashrate ?? 0,
                    Name = (string) x.Value.name,
                    Port = (int) x.Value.port,
                    Workers = (int?) x.Value.workers ?? 0
                })
                .ToArray();

            var result = new MultiPoolInfo {CurrencyInfos = currencies};
            if (!m_Pools.Any())
                return result;

            try
            {
                result.PoolInfos = GetRegisteredPoolInfos(currenciesJObject);
                return result;
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex);
                return result;
            }
        }

        private IReadOnlyDictionary<Pool, PoolInfo> GetRegisteredPoolInfos(JObject currencies)
        {            
            var statusesJson = DownloadDirectlyOrViaProxy($"{m_ApiUrl}/status");
            dynamic statuses = string.IsNullOrWhiteSpace(statusesJson)
                ? new JObject()
                : JsonConvert.DeserializeObject<JObject>(statusesJson);

            var poolStates = m_Pools
                .Where(x => x.ApiPoolName != null && x.WorkerPassword != null)
                .Select(x => new
                {
                    Pool = x,
                    Currency = x.WorkerPassword != null
                               && x.WorkerPassword.StartsWith("c=")
                               && x.WorkerPassword.Length > 2
                        ? x.WorkerPassword.Substring(2)
                        : x.Coin.Symbol
                })
                .Select(x => new
                {
                    x.Pool,
                    PoolInfo = (dynamic) (currencies[$"{x.Pool.Coin.Symbol}-{x.Pool.ApiPoolName}"]
                                          ?? currencies[x.Currency]
                                          ?? currencies[x.Pool.Coin.Symbol])
                })
                .Where(x => x.PoolInfo != null)
                .ToDictionary(x => x.Pool, x => new PoolState
                {
                    TotalWorkers = ((int?) x.PoolInfo.workers).GetValueOrDefault(),
                    TotalHashRate = ((long?) x.PoolInfo.hashrate).GetValueOrDefault(),
                    PoolFee = (double?) statuses[x.Pool.ApiPoolName]?.fees
                });

            var poolAccountInfos = m_Pools
                .AsParallel()
                .WithDegreeOfParallelism(4)
                .Select(x => (pool: x, wallet: x.IsAnonymous
                    ? x.UseBtcWallet
                        ? m_BtcMiningTarget.Address
                        : x.Coin.Wallets.FirstOrDefault(y => y.IsMiningTarget)?.Address
                    : x.WorkerLogin))
                .Where(x => !string.IsNullOrWhiteSpace(x.wallet))
                .Select(x =>
                {
                    var accountInfoHtml = m_PoolUrl != null 
                        ? m_WebClient.DownloadString(new Uri(new Uri(m_PoolUrl), "/site/wallet_results?address=" + x.wallet))
                        : null;
                    try
                    {
                        return new
                        {
                            Pool = x.pool,
                            AccountInfo = ParseJsonAccountInfo(
                                m_WebClient.DownloadStringProxied($"{m_ApiUrl}/wallet?address={x.wallet}")),
                            Payments = ParsePoolPayments(accountInfoHtml)
                        };
                    }
                    catch
                    {
                        // Server returns empty result when request limit is reached or wallet not found
                        // Try to parse HTML page with wallet balance if API doesn't work
                        return new
                        {
                            Pool = x.pool,
                            AccountInfo = ParseHtmlAccountInfo(accountInfoHtml),
                            Payments = ParsePoolPayments(accountInfoHtml)
                        };
                    }
                })
                .ToLookup(x => x.Pool);

            return m_Pools
                .LeftOuterJoin(poolStates, x => x, x => x.Key,
                    (x, y) => (pool: x, poolState: y.Value ?? new PoolState()))
                .LeftOuterJoin(poolAccountInfos, x => x.pool, x => x.Key,
                    (x, y) => (x.pool, x.poolState,
                        accountInfo: y.EmptyIfNull()
                            .Aggregate(
                                new PoolAccountInfo(), (z, a) =>
                                {
                                    z.ConfirmedBalance += a.AccountInfo.ConfirmedBalance;
                                    z.UnconfirmedBalance += a.AccountInfo.UnconfirmedBalance;
                                    z.InvalidShares += a.AccountInfo.InvalidShares;
                                    z.ValidShares += a.AccountInfo.ValidShares;
                                    return z;
                                }),
                       payments: y.EmptyIfNull().SelectMany(z => z.Payments)))
                .ToDictionary(x => x.pool, x => new PoolInfo
                {
                    State = x.poolState,
                    AccountInfo = x.accountInfo,
                    PaymentsData = x.payments.ToArray()
                });
        }

        private string DownloadDirectlyOrViaProxy(string url)
        {
            string result;
            try
            {
                result = m_WebClient.DownloadString(url).Trim();
            }
            catch (CorrectHttpException wex) when (wex.Status == HttpStatusCode.Forbidden)
            {
                return m_WebClient.DownloadStringProxied(url);
            }
            // check if request limit has been reached - in this case try through proxy
            if (result == "" || result.Equals("limit", StringComparison.InvariantCultureIgnoreCase))
                return m_WebClient.DownloadStringProxied(url);
            return result;
        }

        private static PoolAccountInfo ParseJsonAccountInfo(string accountInfoString)
        {
            dynamic workerJson = JsonConvert.DeserializeObject(accountInfoString);
            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = (double) workerJson.balance,
                UnconfirmedBalance = ((double?) workerJson.unsold).GetValueOrDefault()
            };
            var miner = workerJson.miners?.Count > 0 ? workerJson.miners[0] : null;
            if (miner == null)
                return accountInfo;
            accountInfo.ValidShares = (int)(double)miner.accepted;
            accountInfo.InvalidShares = (int)(double)miner.rejected;
            return accountInfo;
        }

        private static PoolAccountInfo ParseHtmlAccountInfo(string accountInfoString)
        {
            if (string.IsNullOrWhiteSpace(accountInfoString))
                return new PoolAccountInfo();

            var html = new HtmlDocument();
            html.LoadHtml(accountInfoString);
            var columns = html.DocumentNode.SelectNodes("//tr[@class='ssrow'][1]/td");
            // Columns: 
            // 0 - logo, 1 - coin name, 2 - immature, 3 - confirmed, 4 - total, 5 - value
            return new PoolAccountInfo
            {
                ConfirmedBalance = ParseColumnValue(columns[3].InnerText),
                UnconfirmedBalance = ParseColumnValue(columns[2].InnerText)
            };

            double ParseColumnValue(string strValue)
                => string.IsNullOrWhiteSpace(strValue) ? 0 : ParsingHelper.ParseValueWithUnits(strValue);
        }

        private static PoolPaymentData[] ParsePoolPayments(string poolPaymentsHtml)
        {
            if (string.IsNullOrWhiteSpace(poolPaymentsHtml))
                return new PoolPaymentData[0];

            var html = new HtmlDocument();
            html.LoadHtml(poolPaymentsHtml);

            var payments = html.DocumentNode
                .SelectNodes("//div[contains(.,'24 Hours Payouts')]/ancestor::div//table/tr[@class='ssrow']")
                .EmptyIfNull()
                .Reverse()
                .Skip(1)
                .Reverse()
                .Select(x => new PoolPaymentData
                {
                    Type = PoolPaymentType.Reward,
                    Amount = ParsingHelper.ParseDouble(x.SelectSingleNode(".//td[2]").InnerText),
                    DateTime = DateTimeHelper.FromIso8601(
                        x.SelectSingleNode(".//td[1]//span").GetAttributeValue("title", null)),
                    Transaction = x.SelectSingleNode(".//td[3]").InnerText.TrimEnd('.')
                })
                .ToArray();
            return payments;
        }
    }
}
