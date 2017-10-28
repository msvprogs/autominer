using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Rig.Data;
using Msv.AutoMiner.Rig.Infrastructure.Contracts;
using NLog;

namespace Msv.AutoMiner.Rig.Infrastructure
{
    public class AutomaticMinerChanger : IAutomaticMinerChanger, IDisposable
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        public CoinMiningData[] CurrentMiningProfitabilityTable { get; private set; }

        private readonly IMinerProcessController m_ProcessController;
        private readonly IMiningProfitabilityTableBuilder m_MiningProfitabilityTableBuilder;
        private readonly IPoolStatusProvider m_PoolStatusProvider;
        private readonly IPeriodicTaskDelayProvider m_DelayProvider;
        private readonly MinerChangingOptions m_ChangingOptions;

        private readonly IDisposable m_Disposable;

        private volatile CoinMiningData m_CurrentCoinData;

        public AutomaticMinerChanger(
            IMinerProcessController processController,
            IMiningProfitabilityTableBuilder miningProfitabilityTableBuilder,
            IPoolStatusProvider poolStatusProvider,
            IPeriodicTaskDelayProvider delayProvider,
            MinerChangingOptions changingOptions)
        {
            m_ProcessController = processController ?? throw new ArgumentNullException(nameof(processController));
            m_MiningProfitabilityTableBuilder = miningProfitabilityTableBuilder ?? throw new ArgumentNullException(nameof(miningProfitabilityTableBuilder));
            m_PoolStatusProvider = poolStatusProvider ?? throw new ArgumentNullException(nameof(poolStatusProvider));
            m_DelayProvider = delayProvider ?? throw new ArgumentNullException(nameof(delayProvider));
            m_ChangingOptions = changingOptions ?? throw new ArgumentNullException(nameof(changingOptions));
            m_Disposable = CreateSubscription();
        }

        public void Dispose() => m_Disposable.Dispose();

        private IDisposable CreateSubscription()
        {
            var random = new Random();
            var delay = m_DelayProvider.GetDelay<AutomaticMinerChanger>();
            M_Logger.Info($"Adding server load balancing delay {(int)delay.TotalSeconds} seconds");
            var intervalDispersionMsec = (int) m_ChangingOptions.Dispersion.TotalMilliseconds;
            return Observable.Generate(Unit.Default, x => true, x => x, x => x,
                    x => m_ChangingOptions.Interval
                         + TimeSpan.FromMilliseconds(random.Next(-intervalDispersionMsec, intervalDispersionMsec)),
                    TaskPoolScheduler.Default)
                .StartWith(TaskPoolScheduler.Default, Unit.Default)
                .Delay(delay)
                .Merge(Observable.FromEventPattern(
                        x => m_ProcessController.ProcessExited += x,
                        x => m_ProcessController.ProcessExited -= x)
                    .Do(x => m_CurrentCoinData = null)
                    .Select(x => Unit.Default))
                .Subscribe(x =>
                {
                    try
                    {
                        DoChanging();
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, "Profitability recheck failed");
                    }
                },
                x => M_Logger.Fatal(x, "Unhandled exception in Rx"));
        }

        private void DoChanging()
        {
            M_Logger.Info("Building mining profitability table...");
            var profitabilityTable = m_MiningProfitabilityTableBuilder.Build();
            CurrentMiningProfitabilityTable = profitabilityTable;

            var mostProfitable = profitabilityTable
                .OrderByDescending(x => x.PoolData.BtcPerDay)
                .Where(x => x.PoolData.BtcPerDay >= 0)
                .Where(x => m_PoolStatusProvider.CheckAvailability(x.PoolData))
                .FirstOrDefault();
            if (mostProfitable == null)
            {
                M_Logger.Warn("Couldn't select currency to mine (all are disabled or unavailable)");
                if (m_CurrentCoinData != null)
                    M_Logger.Warn($"Continuing to mine {m_CurrentCoinData.ToFullNameString()}...");
                return;
            }
            if (m_CurrentCoinData == null || !mostProfitable.Equals(m_CurrentCoinData))
            {
                var currentCoinInfo = m_CurrentCoinData != null
                    ? profitabilityTable.FirstOrDefault(x => x.Equals(m_CurrentCoinData))
                    : null;
                if (currentCoinInfo != null)
                {
                    var diffRatio = ConversionHelper.GetDiffRatio(currentCoinInfo.PoolData.BtcPerDay, mostProfitable.PoolData.BtcPerDay);
                    M_Logger.Info($"{mostProfitable.ToFullNameString()} profitability is {diffRatio:F2}% better");
                    if (diffRatio < m_ChangingOptions.ThresholdRatio)
                    {
                        M_Logger.Info(
                            $"Difference is below threshold {m_ChangingOptions.ThresholdRatio:F1}%, continuing to mine the current coin");
                        return;
                    }
                }
                M_Logger.Info(
                    $"Starting to mine new coin: {mostProfitable.ToFullNameWithBtcString()}");
                ChangeToNewCoin(mostProfitable);
            }
            else
            {
                M_Logger.Info(
                    $"Coin {mostProfitable.ToFullNameWithBtcString()} is still most profitable, continuing to mine it...");
            }
        }

        private void ChangeToNewCoin(CoinMiningData mostProfitable)
        {
            m_CurrentCoinData = mostProfitable;
            m_ProcessController.RunNew(m_CurrentCoinData);
        }
    }
}