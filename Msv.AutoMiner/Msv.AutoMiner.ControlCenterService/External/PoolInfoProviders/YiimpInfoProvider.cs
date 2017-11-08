using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class YiimpInfoProvider : IMultiPoolInfoProvider
    {
        private readonly IWebClient m_WebClient;
        private readonly string m_BaseUrl;
        private readonly Pool[] m_Pools;

        public YiimpInfoProvider(IWebClient webClient, string baseUrl, Pool[] pools)
        {
            if (string.IsNullOrEmpty(baseUrl))
                throw new ArgumentException("Value cannot be null or empty.", nameof(baseUrl));

            m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));
            m_BaseUrl = baseUrl;
            m_Pools = pools ?? throw new ArgumentNullException(nameof(pools));
        }

        public IReadOnlyDictionary<Pool, PoolInfo> GetInfo(DateTime minPaymentDate)
        {
            dynamic poolsJson = JsonConvert.DeserializeObject(m_WebClient.DownloadString($"{m_BaseUrl}/status"));

            var poolStates = m_Pools
                .Where(x => x.ApiPoolName != null)
                .Select(x => (pool:x, poolInfo: poolsJson[x.ApiPoolName]))
                .Where(x => x.poolInfo != null)
                .ToDictionary(x => x.pool, x => new PoolState
                {
                    TotalWorkers = (int) x.poolInfo.workers,
                    TotalHashRate = (long) x.poolInfo.hashrate
                });

            var poolAccountInfos = m_Pools
                .Select(x => (pool:x, wallet: x.IsAnonymous ? x.Coin.Wallets.First(y => y.IsMiningTarget).Address : x.WorkerLogin))
                .Select(x => new
                {
                    Pool = x.pool,
                    AccountInfoString = m_WebClient.DownloadString($"{m_BaseUrl}/walletEx?address={x.wallet}")
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.AccountInfoString))
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

        private static PoolAccountInfo ParsePoolAccountInfo(string accountInfoString)
        {
            dynamic workerJson = JsonConvert.DeserializeObject(accountInfoString);
            var accountInfo = new PoolAccountInfo
            {
                ConfirmedBalance = (double) workerJson.balance,
                UnconfirmedBalance = ((double?) workerJson.unsold).GetValueOrDefault()
            };
            var miner = workerJson.miners.Count > 0 ? workerJson.miners[0] : null;
            if (miner == null)
                return accountInfo;
            accountInfo.ValidShares = (int)(double)miner.accepted;
            accountInfo.InvalidShares = (int)(double)miner.rejected;
            return accountInfo;
        }
    }
}
