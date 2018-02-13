using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Exchanges;
using Msv.AutoMiner.FrontEnd.Providers;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class ExchangesController : EntityControllerBase<Exchange, ExchangeModel, ExchangeType>
    {
        public const string ExchangesMessageKey = "ExchangesMessage";

        private static readonly Dictionary<ExchangeType, Uri> M_ExchangeUrls
            = new Dictionary<ExchangeType, Uri>
            {
                [ExchangeType.Bittrex] = new Uri("https://bittrex.com"),
                [ExchangeType.CoinExchange] = new Uri("https://www.coinexchange.io"),
                [ExchangeType.CoinsMarkets] = new Uri("http://coinsmarkets.com"),
                [ExchangeType.Cryptopia] = new Uri("https://www.cryptopia.co.nz"),
                [ExchangeType.LiveCoin] = new Uri("https://www.livecoin.net"),
                [ExchangeType.Novaexchange] = new Uri("https://novaexchange.com"),
                [ExchangeType.Poloniex] = new Uri("https://poloniex.com"),
                [ExchangeType.StocksExchange] = new Uri("https://stocks.exchange"),
                [ExchangeType.TradeSatoshi] = new Uri("https://tradesatoshi.com"),
                [ExchangeType.YoBit] = new Uri("https://yobit.net"),
                [ExchangeType.BtcAlpha] = new Uri("https://btc-alpha.com"),
                [ExchangeType.CryptoBridge] = new Uri("https://wallet.crypto-bridge.org")
            };

        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IWalletBalanceProvider m_WalletBalanceProvider;
        private readonly AutoMinerDbContext m_Context;

        public ExchangesController(
            ICoinValueProvider coinValueProvider, 
            IWalletBalanceProvider walletBalanceProvider,
            AutoMinerDbContext context)
            : base("_ExchangeRowPartial", context)
        {
            m_CoinValueProvider = coinValueProvider;
            m_WalletBalanceProvider = walletBalanceProvider;
            m_Context = context;
        }

        public IActionResult Index()
            => View(GetEntityModels(null));

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
            return PartialView("_ExchangeRowPartial", GetEntityModels(new[] {exchange.Type}).FirstOrDefault());
        }

        [HttpPost]
        public async Task<IActionResult> Add(ExchangeType id)
        {
            if (id == ExchangeType.Unknown)
                return NotFound();
            var exchange = await m_Context.Exchanges
                .FirstOrDefaultAsync(x => x.Type == id);
            if (exchange != null && exchange.Activity != ActivityState.Deleted)
                return Forbid();
            if (exchange == null)
                await m_Context.Exchanges.AddAsync(new Exchange
                {
                    Type = id,
                    Activity = ActivityState.Active
                });
            else
                exchange.Activity = ActivityState.Active;

            await m_Context.SaveChangesAsync();
            TempData[ExchangesMessageKey] = $"Exchange {id} has been successfully added";
            return RedirectToAction("Index");
        }

        protected override void OnDeleting(Exchange entity)
        {
            entity.PrivateKey = null;
            entity.PublicKey = null;
        }

        protected override ExchangeModel[] GetEntityModels(ExchangeType[] ids)
        {
            var wallets = m_Context.Wallets
                .Where(x => x.Activity != ActivityState.Deleted)
                .Where(x => x.ExchangeType != null)
                .Select(x => x.ExchangeType.Value)
                .AsEnumerable()
                .GroupBy(x => x)
                .ToDictionary(x => x.Key, x => x.Count());
            var lastPriceDates = m_CoinValueProvider.GetCurrentCoinValues(false)
                .SelectMany(x => x.ExchangePrices.EmptyIfNull())
                .GroupBy(x => x.Exchange)
                .ToDictionary(x => x.Key, x => (DateTime?)x.Max(y => y.Updated));
            var lastBalanceDates = m_WalletBalanceProvider.GetLastBalances()
                .GroupBy(x => x.Wallet.ExchangeType)
                .Where(x => x.Key != null)
                .ToDictionary(x => x.Key, x => (DateTime?) x.OrderByDescending(y => y.DateTime).First().DateTime);

            var exchangesQuery = m_Context.Exchanges
                .Where(x => x.Activity != ActivityState.Deleted);
            if (!ids.IsNullOrEmpty())
                exchangesQuery = exchangesQuery.Where(x => ids.Contains(x.Type));
            return exchangesQuery
                .Select(x => new {x.Type, x.Activity, HasKeys = x.PrivateKey != null && x.PublicKey != null })
                .AsEnumerable()
                .LeftOuterJoin(wallets, x => x.Type, x => x.Key, (x, y) => (exchange:x, wallets:y.Value))
                .Select(x => new ExchangeModel
                {
                    Type = x.exchange.Type,
                    Url = M_ExchangeUrls.TryGetValue(x.exchange.Type),
                    Activity = x.exchange.Activity,
                    HasKeys = x.exchange.HasKeys,
                    WalletCount = x.wallets,
                    LastBalanceDate = lastBalanceDates.TryGetValue(x.exchange.Type),
                    LastPriceDate = lastPriceDates.TryGetValue(x.exchange.Type)
                })
                .ToArray();
        }
    }
}
