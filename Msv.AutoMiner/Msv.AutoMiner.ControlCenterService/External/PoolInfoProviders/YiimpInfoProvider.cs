using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using HtmlAgilityPack;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
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
            var currenciesJObject = TryGetJsonObject("currencies", "currencies");
            var statusesJObject = m_Pools.Any() 
                ? TryGetJsonObject("status", "coin statuses")
                : new JObject();

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
                result.PoolInfos = GetRegisteredPoolInfos(currenciesJObject, statusesJObject);
                return result;
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex);
                return result;
            }
        }

        private JObject TryGetJsonObject(string request, string objectType)
        {
            try
            {
                var currenciesJson = DownloadDirectlyOrViaProxy($"{m_ApiUrl}/{request}");
                return string.IsNullOrWhiteSpace(currenciesJson)
                    ? new JObject()
                    : JsonConvert.DeserializeObject<JObject>(currenciesJson);
            }
            catch (Exception ex)
            {
                M_Logger.Error(ex, $"Couldn't get {objectType} from Yiimp API {m_ApiUrl}");
                return new JObject();
            }
        }

        private IReadOnlyDictionary<Pool, PoolInfo> GetRegisteredPoolInfos(JObject currencies, dynamic statuses)
        {            
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
                .WithDegreeOfParallelism(2)
                .Select(x => (pool: x, wallet: x.IsAnonymous
                    ? x.UseBtcWallet
                        ? m_BtcMiningTarget.Address
                        : x.Coin.Wallets.FirstOrDefault(y => y.IsMiningTarget)?.Address
                    : x.WorkerLogin))
                .Where(x => !string.IsNullOrWhiteSpace(x.wallet))
                .Select(GetPoolAccountAndPayments)
                .ToLookup(x => x.pool);

            return m_Pools
                .LeftOuterJoin(poolStates, x => x, x => x.Key,
                    (x, y) => (pool: x, poolState: y.Value ?? new PoolState()))
                .LeftOuterJoin(poolAccountInfos, x => x.pool, x => x.Key,
                    (x, y) => (x.pool, x.poolState,
                        accountInfo: y.EmptyIfNull()
                            .Aggregate(
                                new PoolAccountInfo(), (z, a) =>
                                {
                                    z.ConfirmedBalance += a.accountInfo.ConfirmedBalance;
                                    z.UnconfirmedBalance += a.accountInfo.UnconfirmedBalance;
                                    z.InvalidShares += a.accountInfo.InvalidShares;
                                    z.ValidShares += a.accountInfo.ValidShares;
                                    return z;
                                }),
                       payments: y.EmptyIfNull().SelectMany(z => z.payments)))
                .ToDictionary(x => x.pool, x => new PoolInfo
                {
                    State = x.poolState,
                    AccountInfo = x.accountInfo,
                    PaymentsData = x.payments.ToArray()
                });
        }

        private (Pool pool, PoolAccountInfo accountInfo, PoolPaymentData[] payments) GetPoolAccountAndPayments(
            (Pool pool, string wallet) poolWallet)
        {
            var accountInfoHtml = m_PoolUrl != null
                ? m_WebClient.DownloadString(new Uri(new Uri(m_PoolUrl), "/site/wallet_results?address=" + poolWallet.wallet))
                : null;
            var payments = ParsePoolPayments(accountInfoHtml);

            var accountInfo = ParseHtmlAccountInfo(accountInfoHtml);
            if (accountInfo != null) 
                return (poolWallet.pool, accountInfo, payments);

            try
            {
                accountInfo = ParseJsonAccountInfo(
                    m_WebClient.DownloadStringProxied($"{m_ApiUrl}/wallet?address={poolWallet.wallet}"));
            }
            catch
            {
                accountInfo = new PoolAccountInfo();
            }
            return (poolWallet.pool, accountInfo, payments);
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
                return null;

            var html = new HtmlDocument();
            html.LoadHtml(accountInfoString);
            var columns = html.DocumentNode.SelectNodes("//tr[@class='ssrow'][1]/td");
            if (columns == null || columns.Count < 4)
                return null;

            // Columns: 
            // 0 - logo, 1 - coin name, 2 - immature, 3 - confirmed, 4 - total, 5 - value
            return new PoolAccountInfo
            {
                ConfirmedBalance = ParseColumnValue(columns[3].InnerText),
                UnconfirmedBalance = ParseColumnValue(columns[2].InnerText)
            };

            double ParseColumnValue(string strValue)
                => ParsingHelper.TryParseValueWithUnits(strValue, out var result)
                    ? result
                    : 0;
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
                    Type = PoolPaymentType.TransferToWallet,
                    Amount = -ParseAmountOrDefault(x.SelectSingleNode(".//td[2]")?.InnerText),
                    DateTime = ParseDateTimeOrDefault(
                        x.SelectSingleNode(".//td[1]//span")?.GetAttributeValue("title", null)),
                    Transaction = x.SelectSingleNode(".//td[3]")?.InnerText.TrimEnd('.')
                })
                .Where(x => Math.Abs(x.Amount) > 0 && x.DateTime > default(DateTime))
                .ToArray();
            return payments;

            DateTime ParseDateTimeOrDefault(string dateStr)
                => DateTimeHelper.TryFromIso8601(dateStr, out var result)
                    ? result
                    : default;

            double ParseAmountOrDefault(string strValue)
                => ParsingHelper.TryParseDouble(strValue, out var result)
                    ? result
                    : 0;
        }
    }
}
