using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.Home;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class HomeController : Controller
    {
        public const string DifficultyAggregationKey = "HomeDifficultyAggregation";
        public const string MarketPriceAggregationKey = "HomeMarketPriceAggregation";

        private readonly IOverallProfitabilityCalculator m_ProfitabilityCalculator;
        private readonly IMiningWorkBuilder m_MiningWorkBuilder;
        private readonly AutoMinerDbContext m_Context;

        public HomeController(
            IOverallProfitabilityCalculator profitabilityCalculator,
            IMiningWorkBuilder miningWorkBuilder, 
            AutoMinerDbContext context)
        {
            m_ProfitabilityCalculator = profitabilityCalculator;
            m_MiningWorkBuilder = miningWorkBuilder;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var algorithms = m_Context.CoinAlgorithms
                .Where(x => x.Activity == ActivityState.Active)
                .Select(x => new AlgorithmBaseModel
                {
                    Id = x.Id,
                    Name = x.Name,
                    KnownValue = x.KnownValue
                })
                .ToArray();

            return View(new HomeIndexModel
            {
                TotalAlgorithmCapabilities = m_ProfitabilityCalculator.CalculateTotalPower(),
                Algorithms = algorithms,
                CurrentProfitabilities = GetCurrentProfitabilities()
            });
        }

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult GetProfitabilities(
            ValueAggregationType? difficultyAggregation,
            ValueAggregationType? marketPriceAggregation)
        {
            if (difficultyAggregation != null)
                HttpContext.Session.SetEnum(DifficultyAggregationKey, difficultyAggregation.Value);
            if (marketPriceAggregation != null)
                HttpContext.Session.SetEnum(MarketPriceAggregationKey, marketPriceAggregation.Value);

            return PartialView("_ProfitabilityTablePartial", GetCurrentProfitabilities());
        }

        public IActionResult Error()
            => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        private ProfitabilityModel[] GetCurrentProfitabilities()
            => m_MiningWorkBuilder.Build(
                m_ProfitabilityCalculator.BuildProfitabilityTable(
                    HttpContext.Session.GetEnum<ValueAggregationType>(DifficultyAggregationKey),
                    HttpContext.Session.GetEnum<ValueAggregationType>(MarketPriceAggregationKey)),
                false)
            .Select(x => (coin: x, pool: x.Pools.OrderByDescending(y => y.UsdPerDay).FirstOrDefault()))
            .Where(x => x.pool != null)
            .LeftOuterJoin(m_Context.Coins
                    .Where(x => x.Activity == ActivityState.Active)
                    .ToArray(),
                x => x.coin.CoinId, x=> x.Id, (x, y) => (x.coin, x.pool, logo: y.LogoImageBytes))
            .Select(x => new ProfitabilityModel
            {
                UsdPerDay = x.pool.UsdPerDay,
                BtcPerDay = x.pool.BtcPerDay,
                CoinLogo = x.logo,
                CoinName = x.coin.CoinName,
                CoinSymbol = x.coin.CoinSymbol,
                CoinsPerDay = x.pool.CoinsPerDay,
                PoolName = x.pool.Name
            })
            .ToArray();
    }
}
