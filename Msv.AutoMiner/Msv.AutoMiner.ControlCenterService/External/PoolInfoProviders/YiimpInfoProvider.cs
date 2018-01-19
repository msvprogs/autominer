using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class YiimpInfoProvider : IMultiPoolInfoProvider
    {
        private readonly IProxiedWebClient m_WebClient;
        private readonly string m_BaseUrl;
        private readonly Pool[] m_Pools;

        public YiimpInfoProvider(IProxiedWebClient webClient, string baseUrl, Pool[] pools)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = baseUrl;
            m_Pools = pools ?? throw new ArgumentNullException(nameof(pools));
        }

        public IReadOnlyDictionary<Pool, PoolInfo> GetInfo(DateTime minPaymentDate)
        {
            var currenciesJson = DownloadDirectlyOrViaProxy($"{m_BaseUrl}/currencies");
            if (string.IsNullOrWhiteSpace(currenciesJson))
                throw new ExternalDataUnavailableException("Currencies method returned an empty result");
            var currencies = JsonConvert.DeserializeObject<JObject>(currenciesJson);
            var statusesJson = DownloadDirectlyOrViaProxy($"{m_BaseUrl}/status");
            var statuses = string.IsNullOrWhiteSpace(statusesJson)
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
                    PoolInfo = currencies[$"{x.Pool.Coin.Symbol}-{x.Pool.ApiPoolName}"] 
                        ?? currencies[x.Currency]
                        ?? currencies[x.Pool.Coin.Symbol]
                })
                .Where(x => x.PoolInfo != null)
                .ToDictionary(x => x.Pool, x => new PoolState
                {
                    TotalWorkers = x.PoolInfo["workers"].Value<int>(),
                    TotalHashRate = x.PoolInfo["hashrate"].Value<long>(),
                    PoolFee = statuses[x.Pool.ApiPoolName]?["fees"].Value<double>()
                });

            var poolAccountInfos = m_Pools
                .AsParallel()
                .WithDegreeOfParallelism(4)
                .Select(x => (pool:x, wallet: x.IsAnonymous 
                    ? x.Coin.Wallets.FirstOrDefault(y => y.IsMiningTarget)?.Address 
                    : x.WorkerLogin))
                .Where(x => !string.IsNullOrWhiteSpace(x.wallet))
                .Select(x =>
                {
                    try
                    {
                        return new
                        {
                            Pool = x.pool,
                            AccountInfoString = m_WebClient.DownloadStringProxied(
                                $"{m_BaseUrl}/wallet?address={x.wallet}") //was walletEx with share info
                        };
                    }
                    catch
                    {
                        // Server returns empty result when request limit is reached or wallet not found
                        return new {Pool = x.pool, AccountInfoString = (string)null};
                    }
                })
                .Where(x => x.AccountInfoString != null)
                .ToLookup(x => x.Pool, x => ParsePoolAccountInfo(x.AccountInfoString));

            return m_Pools
                .LeftOuterJoin(poolStates, x => x, x => x.Key,
                    (x, y) => (pool:x, poolState: y.Value ?? new PoolState()))
                .LeftOuterJoin(poolAccountInfos, x => x.pool, x => x.Key,
                    (x, y) => (x.pool, x.poolState,
                        accountInfo: y.EmptyIfNull()
                            .Aggregate(
                                new PoolAccountInfo(), (z, a) =>
                                {
                                    z.ConfirmedBalance += a.ConfirmedBalance;
                                    z.UnconfirmedBalance += a.UnconfirmedBalance;
                                    z.InvalidShares += a.InvalidShares;
                                    z.ValidShares += a.ValidShares;
                                    return z;
                                })))
                .ToDictionary(x => x.pool, x => new PoolInfo
                {
                    State = x.poolState,
                    AccountInfo = x.accountInfo
                });
        }

        private string DownloadDirectlyOrViaProxy(string url)
        {
            string result;
            try
            {
                result = m_WebClient.DownloadString(url).Trim();
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError
                                           && ((HttpWebResponse) wex.Response).StatusCode == HttpStatusCode.Forbidden)
            {
                return m_WebClient.DownloadStringProxied(url);
            }
            // check if request limit has been reached - in this case try through proxy
            if (result == "" || result.Equals("limit", StringComparison.InvariantCultureIgnoreCase))
                return m_WebClient.DownloadStringProxied(url);
            return result;
        }

        private static PoolAccountInfo ParsePoolAccountInfo(string accountInfoString)
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
    }
}
