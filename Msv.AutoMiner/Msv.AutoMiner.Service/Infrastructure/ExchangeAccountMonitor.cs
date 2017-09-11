using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.Storage.Contracts;
using NLog;
using Msv.AutoMiner.Commons;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;

namespace Msv.AutoMiner.Service.Infrastructure
{
    public class ExchangeAccountMonitor : IDisposable
    {
        private const string BitCoinSymbol = "BTC";
        private static readonly TimeSpan M_MonitorInterval = TimeSpan.FromMinutes(30);
        private static readonly ILogger M_Logger = LogManager.GetLogger("ExchangeAccountMonitor");

        private readonly IExchangeTraderFactory m_TraderFactory;
        private readonly IExchangeAccountMonitorStorage m_Storage;
        private readonly IDisposable m_Subscription;

        public ExchangeAccountMonitor(IExchangeTraderFactory traderFactory, IExchangeAccountMonitorStorage storage)
        {
            if (traderFactory == null)
                throw new ArgumentNullException(nameof(traderFactory));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));

            m_TraderFactory = traderFactory;
            m_Storage = storage;
            m_Subscription = Observable.Interval(M_MonitorInterval)
                .StartWith(Scheduler.Default, 0)
                .Subscribe(x =>
                {
                    try
                    {
                        DoWork();
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, "Exchange account monitor exception");
                    }
                });
        }

        private void DoWork()
        {
            M_Logger.Debug("Started exchange account balance updating...");

            var coins = m_Storage.GetCoinsWithExchanges();
            var lastDates = m_Storage.GetLastOperationDates();
            var newData = coins
                .Select(x => x.Exchange)
                .Distinct()
                .Select(x =>
                {
                    try
                    {
                        var trader = m_TraderFactory.Create(x);
                        var newOperations = trader.GetOperations(lastDates.TryGetValue(x, new DateTime(2000, 1, 1)));
                        var result = new CoinExchangeAccountData
                        {
                            Exchange = x,
                            Balances = trader.GetBalances().ToDictionary(y => y.CurrencySymbol),
                            NewOperations = newOperations.ToLookup(y => y.CurrencySymbol)
                        };
                        M_Logger.Info($"Received current balances and operations for exchange {x}: " 
                            + $"{result.Balances.Count} balances, {newOperations.Length} new operations");
                        return result;
                    }
                    catch (Exception ex)
                    {
                        M_Logger.Error(ex, $"Couldn't get balances and operations for exchange {x}");
                        return new CoinExchangeAccountData
                        {
                            Exchange = x,
                            Balances = new Dictionary<string, ExchangeAccountBalanceData>()
                        };
                    }
                })
                .Where(x => x.Balances.Any())
                .ToDictionary(x => x.Exchange);

            M_Logger.Debug("Account balances received, storing them...");
            var bitCoinId = m_Storage.GetBitCoinId();
            if (bitCoinId == null)
                M_Logger.Warn("BitCoin is not registered in the DB!");
            StoreReceivedData(coins, bitCoinId, newData);
            M_Logger.Info("Account balances and operations stored successfully");
        }

        private void StoreReceivedData(Coin[] coins, int? bitCoinId, Dictionary<ExchangeType, CoinExchangeAccountData> newData)
        {
            m_Storage.SaveBalances(
                coins.Select(x => new
                    {
                        CoinId = (int?)x.Id,
                        x.Exchange,
                        Balance = newData.TryGetValue(x.Exchange)?.Balances?.TryGetValue(x.CurrencySymbol)
                    })
                    .Concat(newData.Select(x => new
                    {
                        CoinId = bitCoinId,
                        Exchange = x.Key,
                        Balance = x.Value?.Balances?.TryGetValue(BitCoinSymbol)
                    }))
                    .Where(x => x.CoinId != null && x.Balance != null)
                    .Select(x => new ExchangeAccountBalance
                    {
                        CoinId = x.CoinId.Value,
                        Exchange = x.Exchange,
                        DateTime = DateTime.Now,
                        Balance = x.Balance.Available,
                        BalanceOnOrders = x.Balance.OnOrders
                    })
                    .ToArray());

            m_Storage.SaveOperations(
                coins.Select(x => new
                    {
                        CoinId = (int?)x.Id,
                        x.Exchange,
                        x.CurrencySymbol,
                        newData.TryGetValue(x.Exchange)?.NewOperations
                    })
                    .Concat(newData.Select(x => new
                    {
                        CoinId = bitCoinId,
                        Exchange = x.Key,
                        CurrencySymbol = BitCoinSymbol,
                        x.Value?.NewOperations
                    }))
                    .Where(x => x.CoinId != null && x.NewOperations != null)
                    .Select(x => new
                    {
                        x.CoinId,
                        x.Exchange,
                        NewOperations = x.NewOperations.Contains(x.CurrencySymbol)
                            ? x.NewOperations[x.CurrencySymbol].ToArray()
                            : null
                    })
                    .Where(x => x.NewOperations != null)
                    .SelectMany(x => x.NewOperations.Select(y => new ExchangeAccountOperation
                    {
                        CoinId = x.CoinId.Value,
                        Exchange = x.Exchange,
                        Amount = y.Amount,
                        DateTime = y.DateTime
                    }))
                    .ToArray());
        }

        public void Dispose() => m_Subscription.Dispose();

        private class CoinExchangeAccountData
        {
            public ExchangeType Exchange { get; set; }
            public Dictionary<string, ExchangeAccountBalanceData> Balances { get; set; }
            public ILookup<string, ExchangeAccountOperationData> NewOperations { get; set; }
        }
    }
}
