using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Exchanges;
using Msv.AutoMiner.FrontEnd.Providers;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class ExchangesController : Controller
    {
        public const string ExchangesMessageKey = "ExchangesMessage";

        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IWalletBalanceProvider m_WalletBalanceProvider;
        private readonly AutoMinerDbContext m_Context;

        public ExchangesController(
            ICoinValueProvider coinValueProvider, 
            IWalletBalanceProvider walletBalanceProvider,
            AutoMinerDbContext context)
        {
            m_CoinValueProvider = coinValueProvider;
            m_WalletBalanceProvider = walletBalanceProvider;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var wallets = m_Context.Wallets
                .Where(x => x.Activity != ActivityState.Deleted)
                .Where(x => x.ExchangeType != null)
                .Select(x => x.ExchangeType.Value)
                .AsEnumerable()
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            var lastPriceDates = m_CoinValueProvider.GetCurrentCoinValues()
                .SelectMany(x => x.ExchangePrices.EmptyIfNull())
                .GroupBy(x => x.Exchange)
                .ToDictionary(x => x.Key, x => (DateTime?)x.Max(y => y.Updated));
            var lastBalanceDates = m_WalletBalanceProvider.GetLastBalanceDates()
                .ToDictionary(x => x.Key, x => (DateTime?) x.Value);

            var exchanges = m_Context.Exchanges
                .Where(x => x.Activity != ActivityState.Deleted)
                .Select(x => new { x.Type, x.Activity, HasKeys = x.PrivateKey != null && x.PublicKey != null })
                .AsEnumerable()
                .LeftOuterJoin(wallets, x => x.Type, x => x.Key, (x, y) => (exchange:x, wallets:y.Value))
                .Select(x => new ExchangeModel
                {
                    Type = x.exchange.Type,
                    Activity = x.exchange.Activity,
                    HasKeys = x.exchange.HasKeys,
                    WalletCount = x.wallets,
                    LastBalanceDate = lastBalanceDates.TryGetValue(x.exchange.Type),
                    LastPriceDate = lastPriceDates.TryGetValue(x.exchange.Type)
                })
                .ToArray();
            return View(exchanges);
        }

        [HttpPost]
        public async Task<IActionResult> SetKeys(EditKeysModel model)
        {
            var exchange = await m_Context.Exchanges
                .FirstOrDefaultAsync(x => x.Type == model.Exchange);
            if (exchange == null)
                return NotFound();
            exchange.PublicKey = model.PublicKey;
            exchange.PrivateKey = model.PrivateKey;
            await m_Context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(ExchangeType id)
        {
            var exchange = await m_Context.Exchanges.FirstOrDefaultAsync(x => x.Type == id);
            if (exchange == null)
                return NotFound();
            if (exchange.Activity == ActivityState.Active)
                exchange.Activity = ActivityState.Inactive;
            else if (exchange.Activity == ActivityState.Inactive)
                exchange.Activity = ActivityState.Active;

            await m_Context.SaveChangesAsync();

            TempData[ExchangesMessageKey] =
                $"Exchange {id} has been successfully {(exchange.Activity == ActivityState.Active ? "activated" : "deactivated")}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(ExchangeType id)
        {
            var exchange = await m_Context.Exchanges.FirstOrDefaultAsync(x => x.Type == id);
            if (exchange == null)
                return NotFound();
            exchange.Activity = ActivityState.Deleted;
            await m_Context.SaveChangesAsync();

            TempData[ExchangesMessageKey] = $"Exchange {id} has been successfully deleted";
            return RedirectToAction("Index");
        }
    }
}
