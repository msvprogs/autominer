using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Infrastructure.PoolClients;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using NLog;

namespace Msv.AutoMiner.Common.Infrastructure
{
    public class PoolAvailabilityChecker : IPoolAvailabilityChecker
    {       
        protected static ILogger Logger { get; } = LogManager.GetCurrentClassLogger();

        private readonly IWebClient m_WebClient;

        public PoolAvailabilityChecker([NotNull] IWebClient webClient) 
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public virtual PoolAvailabilityState Check(PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm)
        {
            var watch = Stopwatch.StartNew();
            var result = CheckServer(pool, knownCoinAlgorithm);
            if (result != PoolAvailabilityState.Available)
            {
                Logger.Warn($"Pool {pool.Name} is unavailable, status: {result}");
                return result;
            }
            Logger.Info($"Pool {pool.Name} is available, connection & authorization succeeded (response time: {watch.ElapsedMilliseconds} msec)");
            return PoolAvailabilityState.Available;
        }

        private PoolAvailabilityState CheckServer(PoolDataModel pool, KnownCoinAlgorithm? knownCoinAlgorithm)
        {
            try
            {
                Logger.Info($"Pool {pool.Name} ({pool.Url}): checking availability...");
                switch (pool.Protocol)
                {
                    case PoolProtocol.Stratum:
                        return new StratumPoolClient(pool, knownCoinAlgorithm, Logger).CheckAvailability();
                    case PoolProtocol.JsonRpc:
                        return new JsonRpcPoolClient(m_WebClient, pool).CheckAvailability();
                    case PoolProtocol.XpmForAll:
                        return new XpmForAllPoolClient(pool, Logger).CheckAvailability();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(pool), "Unknown pool protocol");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Pool {pool.Name} ({pool.Url}) didn't respond. {ex.Message}");
                return PoolAvailabilityState.NoResponse;
            }
        }
    }
}
