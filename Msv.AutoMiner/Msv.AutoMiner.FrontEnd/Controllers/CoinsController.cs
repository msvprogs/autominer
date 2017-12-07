using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Data;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Providers;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class CoinsController : Controller
    {
        public const string CoinsMessageKey = "CoinsMessage";

        private readonly ICoinNetworkInfoProvider m_NetworkInfoProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IStoredFiatValueProvider m_FiatValueProvider;
        private readonly AutoMinerDbContext m_Context;

        public CoinsController(
            ICoinNetworkInfoProvider networkInfoProvider,
            ICoinValueProvider coinValueProvider,
            IStoredFiatValueProvider fiatValueProvider, 
            AutoMinerDbContext context)
        {
            m_NetworkInfoProvider = networkInfoProvider;
            m_CoinValueProvider = coinValueProvider;
            m_FiatValueProvider = fiatValueProvider;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var lastInfos = m_NetworkInfoProvider.GetCurrentNetworkInfos();
            var lastCoinValues = m_CoinValueProvider.GetCurrentCoinValues();
            var btcUsdRate = m_FiatValueProvider.GetLastBtcUsdValue().Value;
            var coins = m_Context.Coins
                .Include(x => x.Algorithm)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted)
                .AsEnumerable()
                .LeftOuterJoin(lastInfos, x => x.Id, x => x.CoinId,
                    (x, y) => (coin:x, network: y ?? new CoinNetworkInfo()))
                .LeftOuterJoin(lastCoinValues, x => x.coin.Id, x => x.CurrencyId,
                    (x, y) => (x.coin, x.network, value: y ?? new CoinValue()))
                .Select(x => new CoinDisplayModel
                {
                    Id = x.coin.Id,
                    Name = x.coin.Name,
                    Symbol = x.coin.Symbol,
                    Algorithm = new AlgorithmModel
                    {
                        Id = x.coin.AlgorithmId,
                        KnownValue = x.coin.Algorithm.KnownValue,
                        Name = x.coin.Algorithm.Name
                    },
                    ExchangePrices = x.value?.ExchangePrices
                        .EmptyIfNull()
                        .Do(y => y.UsdPrice = y.Price * btcUsdRate)
                        .ToArray(),
                    Activity = x.coin.Activity,
                    BlockReward = x.network.BlockReward,
                    Difficulty = x.network.Difficulty,
                    NetHashRate = x.network.NetHashRate,
                    Height = x.network.Height,
                    LastUpdated = x.network.Created != default
                        ? x.network.Created
                        : (DateTime?) null
                })
                .ToArray();

            return View(new CoinsIndexModel
            {
                Coins = coins,
                BtcUsdRate = (decimal)btcUsdRate
            });
        }

        public async Task<IActionResult> Create() 
            => View("Edit", new CoinEditModel
            {
                Id = Guid.NewGuid(),
                AvailableAlgorithms = await GetAvailableAlgorithms()
            });

        public async Task<IActionResult> Edit(Guid id)
        {
            var coin = await m_Context.Coins
                .Include(x => x.Algorithm)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (coin == null)
                return NotFound();
            var coinModel = new CoinEditModel
            {
                Id = coin.Id,
                Name = coin.Name,
                Symbol = coin.Symbol,
                AlgorithmId = coin.AlgorithmId,
                AvailableAlgorithms = await GetAvailableAlgorithms(),
                NetworkInfoApiType = coin.NetworkInfoApiType,
                CanonicalBlockReward = coin.CanonicalBlockReward,
                NodeLogin = coin.NodeLogin,
                MaxTarget = coin.MaxTarget,
                NodePassword = coin.NodePassword,
                CanonicalBlockTimeSeconds = coin.CanonicalBlockTimeSeconds,
                NetworkApiName = coin.NetworkInfoApiName,
                NetworkApiUrl = coin.NetworkInfoApiUrl,
                RewardCalculationJavaScript = coin.RewardCalculationJavaScript,
                NodeUrl = coin.NodeHost != null
                    ? $"http://{coin.NodeHost}:{coin.NodePort}"
                    : null
            };
            return View(coinModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(CoinEditModel coinModel)
        {
            if (!ModelState.IsValid)
            {
                coinModel.AvailableAlgorithms = await GetAvailableAlgorithms();
                return View("Edit", coinModel);
            }
            var nodeUrl = coinModel.NodeUrl != null
                ? new Uri(coinModel.NodeUrl)
                : null;
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == coinModel.Id)
                ?? m_Context.Coins.Add(new Coin
                       {
                           Activity = ActivityState.Active
                       }).Entity;
            coin.Name = coinModel.Name;
            coin.AlgorithmId = coinModel.AlgorithmId.GetValueOrDefault();
            coin.Symbol = coinModel.Symbol.ToUpperInvariant();
            coin.NetworkInfoApiUrl = coinModel.NetworkApiUrl;
            coin.CanonicalBlockReward = coinModel.CanonicalBlockReward;
            coin.CanonicalBlockTimeSeconds = coinModel.CanonicalBlockTimeSeconds;
            coin.Id = coinModel.Id;
            coin.NetworkInfoApiName = coinModel.NetworkApiName;
            coin.NetworkInfoApiType = coinModel.NetworkInfoApiType;
            coin.NodeHost = nodeUrl?.Host;
            coin.NodePort = (nodeUrl?.Port).GetValueOrDefault();
            coin.NodeLogin = coinModel.NodeLogin;
            coin.NodePassword = coinModel.NodePassword;
            coin.MaxTarget = coinModel.MaxTarget;
            coin.RewardCalculationJavaScript = coinModel.RewardCalculationJavaScript;
            await m_Context.SaveChangesAsync();
            TempData[CoinsMessageKey] = $"Coin {coin.Name} ({coin.Symbol}) has been successfully saved";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == id);
            if (coin == null)
                return NotFound();
            if (coin.Activity == ActivityState.Active)
                coin.Activity = ActivityState.Inactive;
            else if (coin.Activity == ActivityState.Inactive)
                coin.Activity = ActivityState.Active;

            await m_Context.SaveChangesAsync();

            TempData[CoinsMessageKey] =
                $"Coin {coin.Name} ({coin.Symbol}) has been successfully {(coin.Activity == ActivityState.Active ? "activated" : "deactivated")}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var coin = await m_Context.Coins.FirstOrDefaultAsync(x => x.Id == id);
            if (coin == null)
                return NotFound();
            coin.Activity = ActivityState.Deleted;
            await m_Context.SaveChangesAsync();

            TempData[CoinsMessageKey] =
                $"Coin {coin.Name} ({coin.Symbol}) has been successfully deleted";
            return RedirectToAction("Index");
        }

        private Task<AlgorithmModel[]> GetAvailableAlgorithms()
            => m_Context.CoinAlgorithms
                .Select(x => new AlgorithmModel
                {
                    Id = x.Id,
                    KnownValue = x.KnownValue,
                    Name = x.Name
                })
                .ToArrayAsync();
    }
}
