using System;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Models.LedgerSheet;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class LedgerSheetController : Controller
    {
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly AutoMinerDbContext m_Context;

        public LedgerSheetController(ICoinValueProvider coinValueProvider, AutoMinerDbContext context)
        {
            m_CoinValueProvider = coinValueProvider;
            m_Context = context;
        }

        public IActionResult Index(string fromDate, string toDate)
        {
            //todo: temporary solution
            var from = fromDate != null 
                ? DateTime.ParseExact(fromDate, "dd.MM.yyyy", CultureInfo.InvariantCulture) 
                : DateTime.UtcNow.Date;
            var to = toDate != null 
                ? DateTime.ParseExact(toDate, "dd.MM.yyyy", CultureInfo.InvariantCulture) 
                : DateTime.UtcNow.Date;

            var walletBalances = m_Context.WalletOperations
                .AsNoTracking()
                .FromSql(
                    @"SELECT WalletId, SUM(Amount) AS Amount, NULL AS ExternalId, NOW() AS DateTime, NULL AS TargetAddress, NULL AS Transaction
  FROM autominer_srv.WalletOperations
  WHERE Amount > 0 AND DateTime >= @p0 AND DateTime < @p1
  GROUP BY WalletId
UNION ALL
SELECT WalletId, SUM(Amount), NULL AS ExternalId, NOW() AS DateTime, NULL AS TargetAddress, NULL AS Transaction
  FROM autominer_srv.WalletOperations
  WHERE Amount < 0 AND DateTime >= @p0 AND DateTime < @p1
  GROUP BY WalletId", from, to.AddDays(1))
                .ToArray();
            var walletIds = walletBalances
                .Select(x => x.WalletId)
                .Distinct()
                .ToArray();

            var coinPrices = m_CoinValueProvider.GetCurrentCoinValues(false);
            var ledgerItems = m_Context.Wallets
                .Where(x => walletIds.Contains(x.Id) && x.Activity != ActivityState.Deleted)
                .Join(m_Context.Coins, x => x.CoinId, x => x.Id, (x, y) => new
                {
                    Wallet = x,
                    Coin = y
                })
                .AsEnumerable()
                .Join(walletBalances, x => x.Wallet.Id, x => x.WalletId, (x, y) => new
                {
                    x.Wallet,
                    x.Coin,
                    Balances = y
                })
                .GroupBy(x => x.Coin.Id)
                .Select(x => new
                {
                    x.First().Coin,
                    x.First().Wallet,
                    Debit = x.FirstOrDefault(y => y.Balances.Amount > 0),
                    Credit = x.FirstOrDefault(y => y.Balances.Amount < 0)
                })
                .LeftOuterJoin(coinPrices, x => x.Coin.Id, x => x.CurrencyId,
                    (x, y) => (coinBalances:x, btcPrice: y))
                .Select(x => new LedgerSheetItemModel
                {
                    Coin = new CoinBaseModel
                    {
                        Name = x.coinBalances.Coin.Name,
                        Symbol = x.coinBalances.Coin.Symbol,
                        Logo = x.coinBalances.Coin.LogoImageBytes
                    },
                    Debit = x.coinBalances.Debit?.Balances.Amount ?? 0,
                    Credit = -x.coinBalances.Credit?.Balances.Amount ?? 0,
                    CoinBtcPrice = x.btcPrice?.AverageBtcValue ?? 0,
                    Address = x.coinBalances.Wallet.Address
                })
                .ToArray();

            return View(new LedgerSheetIndexModel
            {
                StartDate = from,
                EndDate = to,
                Items = ledgerItems,
                TotalDebitBtc = ledgerItems
                    .GroupBy(x => x.Address)
                    .Select(x => x.First())
                    .Select(x => x.Debit * x.CoinBtcPrice)
                    .DefaultIfEmpty(0)
                    .Sum(),
                TotalCreditBtc = ledgerItems
                    .GroupBy(x => x.Address)
                    .Select(x => x.First())
                    .Select(x => x.Credit * x.CoinBtcPrice)
                    .DefaultIfEmpty(0)
                    .Sum()
            });
        }
    }
}
