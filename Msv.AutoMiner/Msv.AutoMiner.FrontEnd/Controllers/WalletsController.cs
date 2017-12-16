using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Models.Wallets;
using Msv.AutoMiner.FrontEnd.Providers;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class WalletsController : Controller
    {
        public const string WalletsMessageKey = "WalletsMessage";

        private readonly IStoredFiatValueProvider m_FiatValueProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IWalletBalanceProvider m_WalletBalanceProvider;
        private readonly AutoMinerDbContext m_Context;

        public WalletsController(
            IStoredFiatValueProvider fiatValueProvider,
            ICoinValueProvider coinValueProvider,
            IWalletBalanceProvider walletBalanceProvider,
            AutoMinerDbContext context)
        {
            m_FiatValueProvider = fiatValueProvider;
            m_CoinValueProvider = coinValueProvider;
            m_WalletBalanceProvider = walletBalanceProvider;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var lastBalances = m_WalletBalanceProvider.GetLastBalances();
            var coinValues = m_CoinValueProvider.GetCurrentCoinValues(false);

            var wallets = m_Context.Wallets
                .Include(x => x.Coin)
                .AsNoTracking()
                .Where(x => x.ExchangeType == null || x.Exchange.Activity != ActivityState.Deleted)
                .Where(x => x.Coin.Activity != ActivityState.Deleted)
                .Where(x => x.Activity != ActivityState.Deleted)
                .AsEnumerable()
                .LeftOuterJoin(lastBalances, x => x.Id, x => x.WalletId,
                    (x, y) => (wallet: x, balances: y ?? new WalletBalance()))
                .LeftOuterJoin(coinValues, x => x.wallet.CoinId, x => x.CurrencyId,
                    (x, y) => (x.wallet, x.balances, price: y ?? new CoinValue()))
                .Select(x => new WalletDisplayModel
                {
                    Id = x.wallet.Id,
                    Activity = x.wallet.Activity,
                    Address = x.wallet.Address,
                    Coin = new CoinBaseModel
                    {
                        Id = x.wallet.CoinId,
                        Name = x.wallet.Coin.Name,
                        Symbol = x.wallet.Coin.Symbol,
                        Logo = x.wallet.Coin.LogoImageBytes
                    },
                    CoinBtcPrice = x.price.AverageBtcValue,
                    ExchangeType = x.wallet.ExchangeType,
                    Available = x.balances.Balance,
                    Blocked = x.balances.BlockedBalance,
                    Unconfirmed = x.balances.UnconfirmedBalance,
                    IsMiningTarget = x.wallet.IsMiningTarget,
                    LastUpdated = x.balances.DateTime != default
                        ? x.balances.DateTime
                        : (DateTime?)null
                })
                .ToArray();

            var totalBtc = wallets
                .Where(x => x.Coin.Symbol == "BTC")
                .Select(x => x.Available + x.Blocked)
                .DefaultIfEmpty(0)
                .Sum();

            var totalAltcoinBtc = wallets
                .Where(x => x.Coin.Symbol != "BTC")
                .GroupBy(x => x.Address)
                .Select(x => x.First())
                .Select(x => (x.Available + x.Blocked) * x.CoinBtcPrice)
                .DefaultIfEmpty(0)
                .Sum();

            var btcUsdRate = m_FiatValueProvider.GetLastBtcUsdValue();
            return View(new WalletIndexModel
            {
                Wallets = wallets,
                TotalBtc = totalBtc,
                TotalUsd = totalBtc * btcUsdRate.Value,
                TotalAltcoinBtc = totalAltcoinBtc,
                TotalAltcoinUsd = totalAltcoinBtc * btcUsdRate.Value
            });
        }

        public async Task<IActionResult> Create()
            => View("Edit", new WalletEditModel
            {
                AvailableCoins = await GetAvailableCoins()
            });

        public async Task<IActionResult> Edit(int id)
        {
            var wallet = await m_Context.Wallets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (wallet == null)
                return NotFound();
            var poolModel = new WalletEditModel
            {
                Id = wallet.Id,
                CoinId = wallet.CoinId,
                ExchangeType = wallet.ExchangeType,
                Address = wallet.Address,
                AvailableCoins = await GetAvailableCoins()
            };
            return View(poolModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(WalletEditModel walletModel)
        {
            if (!ModelState.IsValid)
            {
                walletModel.AvailableCoins = await GetAvailableCoins();
                return View("Edit", walletModel);
            }
            var wallet = await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == walletModel.Id)
                       ?? m_Context.Wallets.Add(new Wallet
                       {
                           Activity = ActivityState.Active,
                           Created = DateTime.UtcNow
                       }).Entity;
            wallet.CoinId = walletModel.CoinId.GetValueOrDefault();
            wallet.Address = walletModel.Address;
            wallet.ExchangeType = walletModel.ExchangeType;

            if (m_Context.Wallets
                .Where(x => x.Activity == ActivityState.Active)
                .Count(x => x.CoinId == wallet.CoinId) == 0)
                wallet.IsMiningTarget = true;

            await m_Context.SaveChangesAsync();
            TempData[WalletsMessageKey] = $"Wallet {wallet.Address} has been successfully saved";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var wallet = await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == id);
            if (wallet == null)
                return NotFound();
            if (wallet.Activity == ActivityState.Active)
                wallet.Activity = ActivityState.Inactive;
            else if (wallet.Activity == ActivityState.Inactive)
                wallet.Activity = ActivityState.Active;

            await m_Context.SaveChangesAsync();

            TempData[WalletsMessageKey] =
                $"Wallet {wallet.Address} has been successfully {(wallet.Activity == ActivityState.Active ? "activated" : "deactivated")}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SetAsMiningTarget(int id)
        {
            var wallet = await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == id);
            if (wallet == null)
                return NotFound();
            var allWallets = await m_Context.Wallets
                .Where(x => x.CoinId == wallet.CoinId && x.Id != wallet.Id)
                .ToArrayAsync();
            allWallets.ForEach(x => x.IsMiningTarget = false);
            wallet.IsMiningTarget = true;

            await m_Context.SaveChangesAsync();

            TempData[WalletsMessageKey] =
                $"Wallet {wallet.Address} has been successfully set as mining target";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var wallet = await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == id);
            if (wallet == null)
                return NotFound();
            wallet.Activity = ActivityState.Deleted;
            await m_Context.SaveChangesAsync();

            TempData[WalletsMessageKey] = $"Wallet {wallet.Address} has been successfully deleted";
            return RedirectToAction("Index");
        }

        private Task<CoinBaseModel[]> GetAvailableCoins()
            => m_Context.Coins
                .Where(x => x.Activity != ActivityState.Deleted)
                .Select(x => new CoinBaseModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    Symbol = x.Symbol
                })
                .ToArrayAsync();
    }
}
