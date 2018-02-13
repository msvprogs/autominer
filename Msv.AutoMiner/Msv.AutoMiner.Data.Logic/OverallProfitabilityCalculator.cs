using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class OverallProfitabilityCalculator : IOverallProfitabilityCalculator
    {
        private static readonly TimeSpan M_MaxHeartbeatAge = TimeSpan.FromDays(1);

        private readonly IRigHeartbeatProvider m_HeartbeatProvider;
        private readonly IProfitabilityTableBuilder m_TableBuilder;

        public OverallProfitabilityCalculator(
            IRigHeartbeatProvider heartbeatProvider,
            IProfitabilityTableBuilder tableBuilder)
        {
            m_HeartbeatProvider = heartbeatProvider;
            m_TableBuilder = tableBuilder;
        }

        public SingleProfitabilityData[] BuildProfitabilityTable(
            ValueAggregationType difficultyAggregation,
            ValueAggregationType priceAggregation)
        {
            var totalPower = CalculateTotalPower();
            return m_TableBuilder.Build(new ProfitabilityRequest
            {
                AlgorithmDatas = totalPower.Cast<AlgorithmPowerData>().ToArray(),
                DifficultyAggregationType = difficultyAggregation,
                ElectricityCostUsd = totalPower
                    .Select(x => x.ElectricityCostUsd / (x.Power / 1000))
                    .DefaultIfEmpty(0)
                    .Average(),
                PriceAggregationType = priceAggregation
            });
        }

        public AlgorithmPowerDataCost[] CalculateTotalPower()
            => m_HeartbeatProvider.GetLastHeartbeats()
                .Where(x => DateTime.UtcNow - x.Value.entity.Received <= M_MaxHeartbeatAge)
                .SelectMany(x => x.Value.heartbeat.AlgorithmMiningCapabilities
                    .EmptyIfNull()
                    .Select(y => new AlgorithmPowerDataCost
                    {
                        AlgorithmId = y.AlgorithmId,
                        ElectricityCostUsd = x.Value.heartbeat.ElectricityUnitCostUsd * y.Power / 1000,
                        Power = y.Power,
                        NetHashRate = y.NetHashRate
                    }))
                .GroupBy(x => x.AlgorithmId)
                .Select(x => new AlgorithmPowerDataCost
                {
                    AlgorithmId = x.Key,
                    NetHashRate = x.Sum(y => y.NetHashRate),
                    Power = x.Sum(y => y.Power),
                    ElectricityCostUsd = x.Sum(y => y.ElectricityCostUsd)
                })
                .ToArray();
    }
}
