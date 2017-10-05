﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Coins;
using Msv.AutoMiner.FrontEnd.Models.Pools;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class PoolsController : Controller
    {
        public const string PoolsMessageKey = "PoolsMessage";

        private readonly AutoMinerDbContext m_Context;

        public PoolsController(AutoMinerDbContext context)
            => m_Context = context;

        public async Task<IActionResult> Index()
        {
            var lastInfos = await m_Context.PoolAccountStates
                .GroupBy(x => x.PoolId)
                .Select(x => new
                {
                    PoolId = x.Key,
                    PoolInfo = x.OrderByDescending(y => y.DateTime).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.PoolId, x => x.PoolInfo);

            var pools = m_Context.Pools
                .Include(x => x.Coin)
                .Include(x => x.Coin.Algorithm)
                .AsNoTracking()
                .Where(x => x.Activity != ActivityState.Deleted && x.Coin.Activity != ActivityState.Deleted)
                .AsEnumerable()
                .LeftOuterJoin(lastInfos, x => x.Id, x => x.Key,
                    (x, y) => (pool: x, state: y.Value ?? new PoolAccountState()))
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
                    Activity = x.pool.Activity,
                    HasApi = x.pool.ApiProtocol != PoolApiProtocol.None,
                    ConfirmedBalance = x.state.ConfirmedBalance,
                    UnconfirmedBalance = x.state.UnconfirmedBalance,
                    PoolHashRate = x.state.PoolHashRate,
                    PoolWorkers = x.state.PoolWorkers,
                    Url = GetPoolUri(x.pool).ToString(),
                    LastUpdated = x.state.DateTime != default(DateTime)
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
                AvailableCoins = await GetAvailableCoins()
            };
            return View(poolModel);
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
