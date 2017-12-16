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
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Models.Pools;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class PoolsController : Controller
    {
        public const string PoolsMessageKey = "PoolsMessage";

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
        {
            m_CoinValueProvider = coinValueProvider;
            m_PoolInfoProvider = poolInfoProvider;
            m_NetworkInfoProvider = networkInfoProvider;
            m_ProfitabilityCalculator = profitabilityCalculator;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var lastPoolInfos = m_PoolInfoProvider.GetCurrentPoolInfos();
            var coinPrices = m_CoinValueProvider.GetCurrentCoinValues(false);
            var networkInfos = m_NetworkInfoProvider.GetCurrentNetworkInfos(false);

            var pools = m_Context.Pools
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted && x.Coin.Activity != ActivityState.Deleted)
                .AsEnumerable()
                .LeftOuterJoin(lastPoolInfos, x => x.Id, x => x.PoolId,
                    (x, y) => (pool: x, state: y ?? new PoolAccountState()))
                .LeftOuterJoin(coinPrices, x => x.pool.Coin.Id, x => x.CurrencyId,
                    (x, y) => (x.pool, x.state, price: y ?? new CoinValue()))
                .LeftOuterJoin(networkInfos, x => x.pool.Coin.Id, x => x.CoinId,
                    (x, y) => (x.pool, x.state, x.price, networkInfo: y ?? new CoinNetworkInfo()))
                .Select(x => new PoolDisplayModel
                {
                    Id = x.pool.Id,
                    Name = x.pool.Name,
                    Coin = new CoinBaseModel
                    {
                        Id = x.pool.CoinId,
                        Name = x.pool.Coin.Name,
                        Symbol = x.pool.Coin.Symbol,
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
                    Url = GetPoolUri(x.pool).ToString(),
                    TimeToFind = m_ProfitabilityCalculator.CalculateTimeToFind(
                        x.networkInfo.Difficulty, x.pool.Coin.MaxTarget, x.state.PoolHashRate),
                    LastUpdated = x.state.DateTime != default
                        ? x.state.DateTime
                        : (DateTime?)null
                })
                .ToArray();

            return View(pools);
        }

        public async Task<IActionResult> CreateStratum()
            => View("Edit", new PoolEditModel
            {
                ApiProtocol = PoolApiProtocol.None,
                Url = "stratum+tcp://",
                AvailableCoins = await GetAvailableCoins()
            });

        public async Task<IActionResult> CreateSolo()
            => View("Edit", new PoolEditModel
            {
                ApiProtocol = PoolApiProtocol.JsonRpcWallet,
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
                Url = GetPoolUri(pool).ToString(),
                ApiKey = pool.ApiKey,
                ApiPoolName = pool.ApiPoolName,
                ApiPoolUserId = pool.PoolUserId,
                ApiProtocol = pool.ApiProtocol,
                ApiUrl = pool.ApiUrl,
                FeeRatio = pool.FeeRatio,
                IsAnonymous = pool.IsAnonymous,
                WorkerLogin = pool.WorkerLogin,
                WorkerPassword = pool.WorkerPassword,
                TimeZoneCorrectionHours = pool.TimeZoneCorrectionHours,
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
                Url = GetPoolUri(pool).Scheme + "://",
                ApiKey = pool.ApiKey,
                ApiPoolName = pool.ApiPoolName,
                ApiPoolUserId = pool.PoolUserId,
                ApiProtocol = pool.ApiProtocol,
                ApiUrl = pool.ApiUrl,
                FeeRatio = pool.FeeRatio,
                IsAnonymous = pool.IsAnonymous,
                WorkerLogin = pool.WorkerLogin,
                WorkerPassword = pool.WorkerPassword,
                TimeZoneCorrectionHours = pool.TimeZoneCorrectionHours,
                AvailableCoins = await GetAvailableCoins()
            };
            if (pool.ApiProtocol == PoolApiProtocol.Yiimp)
            {
                var originalUrl = GetPoolUri(pool);
                poolModel.Url = $"{originalUrl.Scheme}://{originalUrl.Host}:";
                poolModel.WorkerPassword = "c=";
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
            pool.ApiProtocol = poolModel.ApiProtocol;
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

        [HttpPost]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var pool = await m_Context.Pools.FirstOrDefaultAsync(x => x.Id == id);
            if (pool == null)
                return NotFound();
            if (pool.Activity == ActivityState.Active)
                pool.Activity = ActivityState.Inactive;
            else if (pool.Activity == ActivityState.Inactive)
                pool.Activity = ActivityState.Active;

            await m_Context.SaveChangesAsync();

            TempData[PoolsMessageKey] =
                $"Pool {pool.Name} has been successfully {(pool.Activity == ActivityState.Active ? "activated" : "deactivated")}";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var pool = await m_Context.Pools.FirstOrDefaultAsync(x => x.Id == id);
            if (pool == null)
                return NotFound();
            pool.Activity = ActivityState.Deleted;
            await m_Context.SaveChangesAsync();

            TempData[PoolsMessageKey] = $"Pool {pool.Name} has been successfully deleted";
            return RedirectToAction("Index");
        }

        private static Uri GetPoolUri(Pool pool)
        {
            string scheme;
            switch (pool.Protocol)
            {
                case PoolProtocol.JsonRpc:
                    scheme = Uri.UriSchemeHttp;
                    break;
                case PoolProtocol.Stratum:
                    scheme = "stratum+tcp";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pool.Protocol));
            }
            var host = Uri.IsWellFormedUriString(pool.Host, UriKind.Absolute)
                ? new Uri(pool.Host).Host
                : pool.Host;
            return new UriBuilder {Scheme = scheme, Host = host, Port = pool.Port}.Uri;
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
