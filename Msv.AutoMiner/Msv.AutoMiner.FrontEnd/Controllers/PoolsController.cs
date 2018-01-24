using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Models.Pools;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class PoolsController : EntityControllerBase<Pool, PoolDisplayModel, int>
    {
        public const string PoolsMessageKey = "PoolsMessage";
        public const string ShowZeroValuesKey = "PoolsShowZeroValues";

        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IPoolInfoProvider m_PoolInfoProvider;
        private readonly ICoinNetworkInfoProvider m_NetworkInfoProvider;
        private readonly IProfitabilityCalculator m_ProfitabilityCalculator;
        private readonly AutoMinerDbContext m_Context;

        public PoolsController(
            ICoinValueProvider coinValueProvider,
            IPoolInfoProvider poolInfoProvider,
            ICoinNetworkInfoProvider networkInfoProvider,
            IProfitabilityCalculator profitabilityCalculator,
            AutoMinerDbContext context)
            : base("_PoolRowPartial", context)
        {
            m_CoinValueProvider = coinValueProvider;
            m_PoolInfoProvider = poolInfoProvider;
            m_NetworkInfoProvider = networkInfoProvider;
            m_ProfitabilityCalculator = profitabilityCalculator;
            m_Context = context;
        }

        public IActionResult Index()
            => View(GetEntityModels(null));

        public async Task<IActionResult> CreateStratum()
            => View("Edit", new PoolEditModel
            {
                PoolApiProtocol = PoolApiProtocol.None,
                Url = "stratum+tcp://",
                AvailableCoins = await GetAvailableCoins()
            });

        public async Task<IActionResult> CreateSolo()
            => View("Edit", new PoolEditModel
            {
                PoolApiProtocol = PoolApiProtocol.JsonRpcWallet,
                Url = "http://localhost:",
                AvailableCoins = await GetAvailableCoins()
            });

        public async Task<IActionResult> Edit(int id)
        {
            var pool = await m_Context.Pools
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (pool == null)
                return NotFound();
            var poolModel = new PoolEditModel
            {
                Id = pool.Id,
                CoinId = pool.CoinId,
                Name = pool.Name,
                Priority = pool.Priority,
                Url = pool.GetUrl().ToString(),
                ApiKey = pool.ApiKey,
                ApiPoolName = pool.ApiPoolName,
                ApiPoolUserId = pool.PoolUserId,
                PoolApiProtocol = pool.ApiProtocol,
                ApiUrl = pool.ApiUrl,
                FeeRatio = pool.FeeRatio,
                IsAnonymous = pool.IsAnonymous,
                WorkerLogin = pool.WorkerLogin,
                WorkerPassword = pool.WorkerPassword,
                TimeZoneCorrectionHours = pool.TimeZoneCorrectionHours,
                UseBtcWallet = pool.UseBtcWallet,
                AvailableCoins = await GetAvailableCoins()
            };
            return View(poolModel);
        }

        public async Task<IActionResult> Clone(int originalId)
        {
            var pool = await m_Context.Pools
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == originalId);
            if (pool == null)
                return NotFound();
            var poolModel = new PoolEditModel
            {
                Url = pool.GetUrl().Scheme + "://",
                ApiKey = pool.ApiKey,
                ApiPoolName = pool.ApiPoolName,
                ApiPoolUserId = pool.PoolUserId,
                PoolApiProtocol = pool.ApiProtocol,
                ApiUrl = pool.ApiUrl,
                FeeRatio = pool.FeeRatio,
                IsAnonymous = pool.IsAnonymous,
                WorkerLogin = pool.WorkerLogin,
                WorkerPassword = pool.WorkerPassword,
                TimeZoneCorrectionHours = pool.TimeZoneCorrectionHours,
                UseBtcWallet = pool.UseBtcWallet,
                AvailableCoins = await GetAvailableCoins()
            };
            if (pool.ApiProtocol == PoolApiProtocol.Yiimp)
            {
                var originalUrl = pool.GetUrl();
                poolModel.Url = $"{originalUrl.Scheme}://{originalUrl.Host}:";
                poolModel.WorkerPassword = pool.UseBtcWallet
                    ? "c=BTC"
                    : "c=";
                poolModel.ApiPoolName = null;
            }
            return View("Edit", poolModel);
        }

        [HttpPost]
        public async Task<IActionResult> Save(PoolEditModel poolModel)
        {
            if (!ModelState.IsValid)
            {
                poolModel.AvailableCoins = await GetAvailableCoins();
                return View("Edit", poolModel);
            }
            var pool = await m_Context.Pools.FirstOrDefaultAsync(x => x.Id == poolModel.Id)
                       ?? m_Context.Pools.Add(new Pool
                       {
                           Activity = ActivityState.Active
                       }).Entity;
            pool.CoinId = poolModel.CoinId.GetValueOrDefault();
            pool.ApiProtocol = poolModel.PoolApiProtocol;
            pool.ApiKey = poolModel.ApiKey;
            pool.ApiUrl = poolModel.ApiUrl;
            pool.ApiPoolName = poolModel.ApiPoolName;
            pool.PoolUserId = poolModel.ApiPoolUserId;
            pool.FeeRatio = poolModel.FeeRatio;
            pool.IsAnonymous = poolModel.IsAnonymous;
            pool.WorkerLogin = poolModel.WorkerLogin;
            pool.WorkerPassword = poolModel.WorkerPassword;
            pool.Priority = poolModel.Priority;
            pool.Name = poolModel.Name;
            pool.TimeZoneCorrectionHours = poolModel.TimeZoneCorrectionHours;
            pool.UseBtcWallet = poolModel.UseBtcWallet;

            var poolUrl = new Uri(poolModel.Url);
            pool.Host = poolUrl.Host;
            pool.Port = poolUrl.Port;
            pool.Protocol = poolUrl.Scheme.Equals("stratum+tcp", StringComparison.CurrentCultureIgnoreCase)
                ? PoolProtocol.Stratum
                : PoolProtocol.JsonRpc;

            await m_Context.SaveChangesAsync();
            TempData[PoolsMessageKey] = $"Pool {pool.Name} has been successfully saved";
            return RedirectToAction("Index");
        }

        public IActionResult ToggleShowZero()
        {
            HttpContext.Session.SetBool(ShowZeroValuesKey, !HttpContext.Session.GetBool(ShowZeroValuesKey).GetValueOrDefault(true));
            return RedirectToAction("Index");
        }

        protected override PoolDisplayModel[] GetEntityModels(int[] ids)
        {
            var lastPoolInfos = m_PoolInfoProvider.GetCurrentPoolInfos();
            var coinPrices = m_CoinValueProvider.GetCurrentCoinValues(false);
            var networkInfos = m_NetworkInfoProvider.GetCurrentNetworkInfos(false);

            var poolQuery = m_Context.Pools
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted && x.Coin.Activity != ActivityState.Deleted);
            if (!ids.IsNullOrEmpty())
                poolQuery = poolQuery.Where(x => ids.Contains(x.Id));

            return poolQuery
                .AsEnumerable()
                .LeftOuterJoin(lastPoolInfos, x => x.Id, x => x.PoolId,
                    (x, y) => (pool: x, state: y ?? new PoolAccountState()))
                .LeftOuterJoin(coinPrices, x => x.pool.Coin.Id, x => x.CurrencyId,
                    (x, y) => (x.pool, x.state, price: y ?? new CoinValue()))
                .LeftOuterJoin(networkInfos, x => x.pool.Coin.Id, x => x.CoinId,
                    (x, y) => (x.pool, x.state, x.price, networkInfo: y ?? new CoinNetworkInfo()))
                .Where(x => HttpContext.Session.GetBool(ShowZeroValuesKey).GetValueOrDefault(true)
                            || x.state.ConfirmedBalance > 0
                            || x.state.UnconfirmedBalance > 0)
                .Select(x => new PoolDisplayModel
                {
                    Id = x.pool.Id,
                    Name = x.pool.Name,
                    Coin = new CoinBaseModel
                    {
                        Id = x.pool.CoinId,
                        Name = x.pool.Coin.Name,
                        Symbol = x.pool.Coin.Symbol,
                        Logo = x.pool.Coin.LogoImageBytes,
                        Algorithm = new AlgorithmModel { KnownValue = x.pool.Coin.Algorithm.KnownValue }
                    },
                    CoinBtcPrice = x.price.AverageBtcValue,
                    Activity = x.pool.Activity,
                    HasApi = x.pool.ApiProtocol != PoolApiProtocol.None,
                    IsSolo = x.pool.Protocol == PoolProtocol.JsonRpc,
                    ConfirmedBalance = x.state.ConfirmedBalance,
                    UnconfirmedBalance = x.state.UnconfirmedBalance,
                    PoolHashRate = x.state.PoolHashRate,
                    PoolWorkers = x.state.DateTime != default
                        ? x.state.PoolWorkers
                        : (int?)null,
                    Fee = x.pool.FeeRatio,
                    Url = x.pool.GetUrl().ToString(),
                    TimeToFind = m_ProfitabilityCalculator.CalculateTimeToFind(
                        x.networkInfo.Difficulty, x.pool.Coin.MaxTarget, x.state.PoolHashRate),
                    LastUpdated = x.state.DateTime != default
                        ? x.state.DateTime
                        : (DateTime?)null,
                    ResponsesStoppedDate = x.pool.ResponsesStoppedDate,
                    UseBtcWallet = x.pool.UseBtcWallet
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
