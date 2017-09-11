using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using Msv.AutoMiner.Service.Infrastructure.Data;
using NLog;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class AutomaticMinerChanger : IAutomaticMinerChanger, IDisposable
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("AutomaticMinerChanger");

        public CoinProfitabilityData[] CurrentProfitabilityTable { get; private set; }

        public MiningMode CurrentMode => m_CurrentCoinData?.Mode ?? MiningMode.Stopped;

        public CoinBaseInfo[] CurrentCoins => m_CurrentCoinData?.Coins
            .Select(x => new CoinBaseInfo(x.Coin.Name, x.Coin.CurrencySymbol, x.Coin.Algorithm))
            .ToArray();

        private readonly IMinerProcessController m_ProcessController;
        private readonly IProfitabilityTableBuilder m_ProfitabilityTableBuilder;
        private readonly IAutomaticMinerChangerStorage m_Storage;
        private readonly IPoolStatusProvider m_PoolStatusProvider;
        private readonly MinerChangingOptions m_ChangingOptions;

        private readonly IDisposable m_Disposable;

        private volatile CoinProfitabilityData m_CurrentCoinData;

        public AutomaticMinerChanger(
            IMinerProcessController processController,
            IProfitabilityTableBuilder profitabilityTableBuilder,
            IAutomaticMinerChangerStorage storage,
            IPoolStatusProvider poolStatusProvider,
            MinerChangingOptions changingOptions)
        {
            if (processController == null)
                throw new ArgumentNullException(nameof(processController));
            if (profitabilityTableBuilder == null)
                throw new ArgumentNullException(nameof(profitabilityTableBuilder));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (poolStatusProvider == null)
                throw new ArgumentNullException(nameof(poolStatusProvider));
            if (changingOptions == null)
                throw new ArgumentNullException(nameof(changingOptions));

            m_ProcessController = processController;
            m_ProfitabilityTableBuilder = profitabilityTableBuilder;
            m_Storage = storage;
            m_PoolStatusProvider = poolStatusProvider;
            m_ChangingOptions = changingOptions;
            m_Disposable = CreateSubscription();
        }

        public void Dispose()
        {
            m_Disposable.Dispose();
        }

        private IDisposable CreateSubscription()
        {
            var random = new Random();
            var intervalDispersionMsec = (int) m_ChangingOptions.Dispersion.TotalMilliseconds;
            return Observable.Generate(Unit.Default, x => true, x => x, x => x,
                    x => m_ChangingOptions.Interval +
                         TimeSpan.FromMilliseconds(random.Next(-intervalDispersionMsec, intervalDispersionMsec)),
                    Scheduler.Default)
                .Merge(Observable.FromEventPattern(
                    x => m_ProcessController.ProcessExited += x, 
                    x => m_ProcessController.ProcessExited -= x)
                    .Do(x => m_CurrentCoinData = null)
                    .Select(x => Unit.Default))
                .StartWith(Scheduler.Default, Unit.Default)
                .Subscribe(x =>
                {
                    try
                    {
                        DoChanging();
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, "Profitability recheck failed: ");
                    }
                });
        }

        private void DoChanging()
        {
            M_Logger.Info("Building profitability table...");
            var profitabilityTable = m_ProfitabilityTableBuilder.Build();
            CurrentProfitabilityTable = profitabilityTable;
            m_Storage.SaveProfitabilities(profitabilityTable
                .Where(x => x.Mode == MiningMode.Single)
                .Select(x => new CoinProfitability
                {
                    CoinId = x.Coins.Single().Coin.Id,
                    DateTime = DateTime.Now,
                    BtcPerDay = x.BtcPerDay,
                    UsdPerDay = x.UsdPerDayTotal,
                    CoinsPerDay = x.Coins.Single().CoinsPerDay
                })
                .ToArray());

            var mostProfitable = profitabilityTable
                .Where(x => x.Coins.All(y => y.Coin.Activity == ActivityState.Active))
                .OrderByDescending(x => x.BtcPerDay)
                .Where(x => x.Coins.Select(y => y.Coin.PoolId)
                    .Distinct()
                    .All(y => m_PoolStatusProvider.CheckAvailability(y.GetValueOrDefault())))
                .FirstOrDefault();
            if (mostProfitable == null)
            {
                M_Logger.Warn("Couldn't select currency to mine (all are disabled or unavailable)");
                if (m_CurrentCoinData != null)
                    M_Logger.Warn($"Continuing to mine {m_CurrentCoinData?.ToCoinNameString()}...");
                return;
            }
            if (m_CurrentCoinData == null || mostProfitable.Id != m_CurrentCoinData.Id)
            {
                var currentCoinInfo = m_CurrentCoinData != null
                    ? profitabilityTable.Where(x => x.Coins.All(y => y.Coin.Activity == ActivityState.Active))
                        .FirstOrDefault(x => x.Id == m_CurrentCoinData.Id)
                    : null;
                if (currentCoinInfo != null)
                {
                    var diffRatio = ConversionHelper.GetDiffRatio(currentCoinInfo.BtcPerDay, mostProfitable.BtcPerDay);
                    M_Logger.Info($"{mostProfitable.ToCoinString()} profitability is {diffRatio:F2}% better");
                    if (diffRatio < m_ChangingOptions.ThresholdRatio)
                    {
                        M_Logger.Info(
                            $"Difference is below threshold {m_ChangingOptions.ThresholdRatio:F1}%, continuing to mine the current coin");
                        return;
                    }
                }
                M_Logger.Info(
                    $"Starting to mine new coin: {mostProfitable.ToCoinString()} ({mostProfitable.BtcPerDay:F6} BTC/day)");
                ChangeToNewCoin(currentCoinInfo, mostProfitable);
            }
            else
            {
                M_Logger.Info(
                    $"Coin {mostProfitable.ToCoinString()} ({mostProfitable.BtcPerDay:F6} BTC/day) is still most profitable, continuing to mine it...");
            }
        }

        private void ChangeToNewCoin(CoinProfitabilityData currentCoinData, CoinProfitabilityData mostProfitable)
        {
            var date = DateTime.Now;
            if (currentCoinData != null)
                m_Storage.SaveChangeEvents(currentCoinData.Coins
                    .Select(x => new MiningChangeEvent
                    {
                        DateTime = date,
                        FromCoinId = x.Coin.Id,
                        SourceProfitability = (decimal?) currentCoinData.BtcPerDay,
                        TargetProfitability = (decimal) mostProfitable.BtcPerDay,
                        ToCoinId = mostProfitable.Coins.First().Coin.Id
                    })
                    .ToArray());
            m_CurrentCoinData = mostProfitable;
            m_ProcessController.RunNew(
                m_CurrentCoinData.Coins.Select(x => x.Coin).ToArray(),
                m_CurrentCoinData.Miner);
        }
    }
}