using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Models.Wallets;
using Msv.AutoMiner.FrontEnd.Providers;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class WalletsController : Controller
    {
        public const string WalletsMessageKey = "WalletsMessage";
        public const string ShowZeroValuesKey = "WalletsShowZeroValues";

        private readonly IStoredFiatValueProvider m_FiatValueProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IWalletBalanceProvider m_WalletBalanceProvider;
        private readonly IWalletAddressValidatorFactory m_AddressValidatorFactory;
        private readonly AutoMinerDbContext m_Context;

        public WalletsController(
            IStoredFiatValueProvider fiatValueProvider,
            ICoinValueProvider coinValueProvider,
            IWalletBalanceProvider walletBalanceProvider,
            IWalletAddressValidatorFactory addressValidatorFactory,
            AutoMinerDbContext context)
        {
            m_FiatValueProvider = fiatValueProvider;
            m_CoinValueProvider = coinValueProvider;
            m_WalletBalanceProvider = walletBalanceProvider;
            m_AddressValidatorFactory = addressValidatorFactory;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var wallets = GetWalletDisplayModels(null);

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
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == walletModel.CoinId);
            if (coin == null)
                return NotFound();

            var addressValidator = m_AddressValidatorFactory.Create(
                coin.AddressFormat,
                string.IsNullOrEmpty(coin.AddressPrefixes)
                    ? new string[0]
                    : coin.AddressPrefixes.Split(',').Select(x => x.Trim()).ToArray());
            if (!addressValidator.HasCheckSum(walletModel.Address))
                ModelState.AddModelError(nameof(walletModel.Address), "Your address doesn't contain checksum");
            else if (!addressValidator.IsValid(walletModel.Address))
                ModelState.AddModelError(nameof(walletModel.Address), "Incorrect address");

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

            if (!m_Context.Wallets
                    .Where(x => x.Activity == ActivityState.Active)
                    .Any(x => x.CoinId == wallet.CoinId)
                || walletModel.SetAsMiningTarget)
                await SetWalletAsMiningTarget(wallet);

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
            return PartialView("_WalletRowsPartial", GetWalletDisplayModels(new[] {id}));
        }

        [HttpPost]
        public async Task<IActionResult> SetAsMiningTarget(int id)
        {
            var wallet = await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == id);
            if (wallet == null)
                return NotFound();

            var allWalletIds = await SetWalletAsMiningTarget(wallet);
            await m_Context.SaveChangesAsync();
            return PartialView("_WalletRowsPartial", GetWalletDisplayModels(allWalletIds));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var wallet = await m_Context.Wallets.FirstOrDefaultAsync(x => x.Id == id);
            if (wallet == null)
                return NotFound();
            wallet.Activity = ActivityState.Deleted;

            await m_Context.SaveChangesAsync();
            return NoContent();
        }

        public IActionResult ToggleShowZero()
        {
            HttpContext.Session.SetBool(ShowZeroValuesKey, !HttpContext.Session.GetBool(ShowZeroValuesKey).GetValueOrDefault(true));
            return RedirectToAction("Index");
        }

        private async Task<int[]> SetWalletAsMiningTarget(Wallet wallet)
        {
            var allWallets = await m_Context.Wallets
                .Where(x => x.CoinId == wallet.CoinId && x.Id != wallet.Id)
                .ToArrayAsync();
            allWallets.ForEach(x => x.IsMiningTarget = false);
            wallet.IsMiningTarget = true;
            return allWallets.Select(x => x.Id).Concat(new[] {wallet.Id}).ToArray();
        }

        private WalletDisplayModel[] GetWalletDisplayModels(int[] ids)
        {
            var lastBalances = m_WalletBalanceProvider.GetLastBalances();
            var coinValues = m_CoinValueProvider.GetCurrentCoinValues(false);

            var walletQuery = m_Context.Wallets
                .Include(x => x.Coin)
                .AsNoTracking()
                .Where(x => x.ExchangeType == null || x.Exchange.Activity != ActivityState.Deleted)
                .Where(x => x.Coin.Activity != ActivityState.Deleted)
                .Where(x => x.Activity != ActivityState.Deleted);
            if (!ids.IsNullOrEmpty())
                walletQuery = walletQuery.Where(x => ids.Contains(x.Id));

            return walletQuery
                .AsEnumerable()
                .LeftOuterJoin(lastBalances, x => x.Id, x => x.WalletId,
                    (x, y) => (wallet: x, balances: y ?? new WalletBalance()))
                .LeftOuterJoin(coinValues, x => x.wallet.CoinId, x => x.CurrencyId,
                    (x, y) => (x.wallet, x.balances, price: y ?? new CoinValue(), miningMarket: y?.ExchangePrices
                        .EmptyIfNull()
                        .FirstOrDefault(z => z.Exchange == x.wallet.ExchangeType)))
                .Where(x => HttpContext.Session.GetBool(ShowZeroValuesKey).GetValueOrDefault(true)
                    || x.balances.Balance > 0
                    || x.balances.BlockedBalance > 0 
                    || x.balances.UnconfirmedBalance > 0)
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
                    LastDayVolume = x.miningMarket?.VolumeBtc,
                    IsInactive = x.miningMarket != null && !x.miningMarket.IsActive,
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
