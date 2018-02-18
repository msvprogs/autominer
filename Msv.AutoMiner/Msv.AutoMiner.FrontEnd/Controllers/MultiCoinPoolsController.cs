using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.FrontEnd.Models.MultiCoinPools;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class MultiCoinPoolsController : EntityControllerBase<MultiCoinPool, MultiCoinPoolDisplayModel, int>
    {
        public const string MultiCoinPoolsMessageKey = "MultiCoinPoolsMessage";
        private const string IgnoredAlgorithmsDelimiter = ",";

        private readonly IStoredSettings m_Settings;
        private readonly AutoMinerDbContext m_Context;

        public MultiCoinPoolsController(IStoredSettings settings, AutoMinerDbContext context) 
            : base("_MultiCoinPoolRowPartial", context)
        {
            m_Settings = settings;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var currencyExchanges = m_Context.ExchangeCurrencies
                .AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.Symbol)
                .ToDictionary(x => x.Key, x => x
                    .Select(y => y.Exchange.ToString())
                    .Distinct()
                    .OrderBy(y => y)
                    .ToArray());
            var currentCoinsWithPools = m_Context.Pools
                .AsNoTracking()
                .Include(x => x.Coin)
                .Where(x => x.Activity == ActivityState.Active
                            && x.Coin.Activity == ActivityState.Active)
                .Select(x => x.Coin.Symbol)
                .Distinct()
                .ToArray();
            var ignoredAlgorithms = m_Settings.MultiPoolIgnoreAlgorithms.EmptyIfNull()
                .Split(IgnoredAlgorithmsDelimiter);

            return View(new MultiCoinPoolIndexModel
            {
                Pools = GetEntityModels(null),
                IgnoredAlgorithms = ignoredAlgorithms,
                Currencies = m_Context.MultiCoinPoolCurrencies
                    .Include(x => x.MultiCoinPool)
                    .AsNoTracking()
                    .Where(x => !x.IsIgnored)
                    .Where(x => x.MultiCoinPool.Activity != ActivityState.Deleted)
                    .Where(x => !currentCoinsWithPools.Contains(x.Symbol))
                    .Where(x => !ignoredAlgorithms.Contains(x.Algorithm, StringComparer.InvariantCultureIgnoreCase))
                    .AsEnumerable()
                    .Select(x => new MultiCoinPoolCurrencyModel
                    {
                        Name = x.Name,
                        Algorithm = x.Algorithm,
                        Hashrate = x.Hashrate,
                        Id = x.Id,
                        MultiCoinPoolId = x.MultiCoinPoolId,
                        MultiCoinPoolName = x.MultiCoinPool.Name,
                        MultiCoinPoolApiUrl = x.MultiCoinPool.ApiUrl,
                        MultiCoinPoolSiteUrl = x.MultiCoinPool.SiteUrl,
                        MiningUrl = CreatMiningUrl(x.MultiCoinPool.MiningUrl, x.Port, x.Algorithm)?.ToString(),
                        Symbol = x.Symbol,
                        Workers = x.Workers,
                        Exchanges = currencyExchanges.TryGetValue(x.Symbol).EmptyIfNull()
                    })
                    .Where(x => x.Exchanges.Any())
                    .ToArray()
            });
        }

        public IActionResult Create()
            => View("Edit", new MultiCoinPoolEditModel());

        public async Task<IActionResult> Edit(int id)
        {
            var pool = await m_Context.MultiCoinPools
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
            if (pool == null)
                return NotFound();
            return View(new MultiCoinPoolEditModel
            {
                Name = pool.Name,
                ApiProtocol = pool.ApiProtocol,
                ApiUrl = pool.ApiUrl,
                Id = pool.Id,
                MiningUrl = pool.MiningUrl,
                SiteUrl = pool.SiteUrl
            });
        }

        [HttpPost]
        public IActionResult IgnoreNewCurrency(int id)
        {
            var currency = m_Context.MultiCoinPoolCurrencies
                .FirstOrDefault(x => x.Id == id);
            if (currency == null)
                return NotFound();
            currency.IsIgnored = true;
            m_Context.SaveChanges();
            return NoContent();
        }

        [HttpPost]
        public IActionResult ChangeIgnoredAlgorithms(string newAlgorithms)
        {
            m_Settings.MultiPoolIgnoreAlgorithms =
                string.Join(IgnoredAlgorithmsDelimiter, newAlgorithms
                    .EmptyIfNull()
                    .Split(IgnoredAlgorithmsDelimiter, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.Trim()));
            TempData[MultiCoinPoolsMessageKey] = "Now ignoring algorithms: " + newAlgorithms;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Save(MultiCoinPoolEditModel poolModel)
        {
            if (!ModelState.IsValid)
                return View("Edit", poolModel);
            var pool = await m_Context.MultiCoinPools.FirstOrDefaultAsync(x => x.Id == poolModel.Id)
                       ?? m_Context.MultiCoinPools.Add(new MultiCoinPool
                       {
                           Activity = ActivityState.Active
                       }).Entity;
            pool.Name = poolModel.Name;
            pool.ApiProtocol = poolModel.ApiProtocol;
            pool.ApiUrl = poolModel.ApiUrl;
            pool.MiningUrl = poolModel.MiningUrl;
            pool.SiteUrl = poolModel.SiteUrl;

            await m_Context.SaveChangesAsync();
            TempData[MultiCoinPoolsMessageKey] = $"Multicoin pool {pool.Name} has been successfully saved";
            return RedirectToAction("Index");
        }

        protected override MultiCoinPoolDisplayModel[] GetEntityModels(int[] ids)
        {
            var multiCoinPoolQuery = m_Context.MultiCoinPools.AsNoTracking();
            if (!ids.IsNullOrEmpty())
                multiCoinPoolQuery = multiCoinPoolQuery.Where(x => ids.Contains(x.Id));
            return multiCoinPoolQuery
                .Where(x => x.Activity != ActivityState.Deleted)
                .Select(x => new MultiCoinPoolDisplayModel
                {
                    Activity = x.Activity,
                    ApiProtocol = x.ApiProtocol,
                    Id = x.Id,
                    Name = x.Name,
                    SiteUrl = x.SiteUrl
                })
                .ToArray();
        }

        private static Uri CreatMiningUrl(string templateUri, int port, string algorithm) 
            => templateUri != null 
                ? new UriBuilder(templateUri.Replace("_algo_", algorithm)) { Port = port }.Uri
                : null;
    }
}
