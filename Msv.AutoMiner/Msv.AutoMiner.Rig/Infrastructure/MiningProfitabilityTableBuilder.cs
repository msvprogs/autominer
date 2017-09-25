using System;
using System.Linq;
using System.Text;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Data;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using Msv.AutoMiner.Rig.Remote;
using Msv.AutoMiner.Rig.Storage.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class MiningProfitabilityTableBuilder : IMiningProfitabilityTableBuilder
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IControlCenterService m_ControlCenterService;
        private readonly IMiningProfitabilityTableBuilderStorage m_Storage;
        private readonly ProfitabilityTableBuilderParams m_Parameters;

        public MiningProfitabilityTableBuilder(
            IControlCenterService controlCenterService,
            IMiningProfitabilityTableBuilderStorage storage,
            ProfitabilityTableBuilderParams parameters)
        {
            m_ControlCenterService = controlCenterService ?? throw new ArgumentNullException(nameof(controlCenterService));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            m_Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public CoinMiningData[] Build()
        {
            M_Logger.Info("Requesting new mining works...");
            var algorithmDatas = m_Storage.GetAlgorithmDatas()
                .ToDictionary(x => x.AlgorithmId);
            var miningWorksTask = m_ControlCenterService.GetMiningWork(
                new GetMiningWorkRequestModel
                {
                    ElectricityCostUsd = m_Parameters.ElectricityKwhCostUsd,
                    AlgorithmDatas = algorithmDatas.Values
                        .Select(x => new AlgorithmPowerData
                        {
                            AlgorithmId = x.AlgorithmId,
                            NetHashRate = x.SpeedInHashes,
                            Power = x.Power
                        })
                        .ToArray()
                });
            var minerSettings = m_Storage.GetAlgorithmSettings()
                .ToDictionary(x => x.AlgorithmId);
            var profitabilityTable = miningWorksTask
                .SelectMany(x => x.Pools.Select(y => new CoinMiningData
                {
                    CoinId = x.CoinId,
                    CoinName = x.CoinName,
                    CoinSymbol = x.CoinSymbol,
                    MinerSettings = minerSettings.TryGetValue(x.CoinAlgorithmId),
                    PoolData = y,
                    PowerUsage = (algorithmDatas.TryGetValue(x.CoinAlgorithmId)?.Power).GetValueOrDefault()
                }))
                .OrderByDescending(x => x.UsdPerDayTotal)
                .ToArray();
            M_Logger.Debug("Current profitability table:"
                           + Environment.NewLine
                           + "Symbol    Name                     Pool                     Coins per day   BTC per day   USD per day  Electricity  Total"
                           + Environment.NewLine
                           + string.Join(Environment.NewLine, profitabilityTable.Select(
                               x => new StringBuilder(x.CoinSymbol.PadRight(8))
                                   .Append(x.CoinName.PadRight(22))
                                   .Append(x.PoolData.Name.PadRight(26))
                                   .Append(x.ToCoinsPerDayString().PadLeft(6))
                                   .Append($"{x.PoolData.BtcPerDay,12:N6}")
                                   .Append($"{x.PoolData.UsdPerDay,10:N2}")
                                   .Append($"{-x.PoolData.ElectricityCost,13:N2}$")
                                   .Append($"{x.UsdPerDayTotal,10:N2}$"))));
            return profitabilityTable;
        }
    }
}
