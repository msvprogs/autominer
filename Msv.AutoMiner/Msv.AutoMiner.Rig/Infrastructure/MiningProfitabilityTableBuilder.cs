using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Rig.Commands;
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
                .ToDictionary(x => Guid.Parse(x.AlgorithmId));
            var miningWorksTask = m_ControlCenterService.GetMiningWork(
                new GetMiningWorkRequestModel
                {
                    ElectricityCostUsd = m_Parameters.ElectricityKwhCostUsd,
                    AlgorithmDatas = algorithmDatas.Values
                        .Select(x => new AlgorithmPowerData
                        {
                            AlgorithmId = Guid.Parse(x.AlgorithmId),
                            NetHashRate = x.SpeedInHashes,
                            Power = x.Power
                        })
                        .ToArray()
                });
            var minerSettings = m_Storage.GetAlgorithmSettings()
                .ToDictionary(x => Guid.Parse(x.AlgorithmId));
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
                .Where(x => x.MinerSettings != null)
                .OrderByDescending(x => x.UsdPerDayTotal)
                .ToArray();

            var tableBuilder = new TableStringBuilder(
                "Symbol", "Name", "Pool", "Coins per day", "BTC per day", "USD per day", "Electricity", "Total $");
            profitabilityTable.ForEach(
                x => tableBuilder.AppendValues(
                    x.CoinSymbol, 
                    x.CoinName,
                    x.PoolData.Name, 
                    x.ToCoinsPerDayString(),
                    ConversionHelper.ToCryptoCurrencyValue(x.PoolData.BtcPerDay), 
                    x.PoolData.UsdPerDay.ToString("N2"),
                    $"-${x.PoolData.ElectricityCost:N2}",
                    $"{x.UsdPerDayTotal:N2}"));
            M_Logger.Debug("Current profitability table: " + Environment.NewLine + tableBuilder);

            return profitabilityTable;
        }
    }
}
