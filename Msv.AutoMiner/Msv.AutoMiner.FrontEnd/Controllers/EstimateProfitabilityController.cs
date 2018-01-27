using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Infrastructure;
using Msv.AutoMiner.FrontEnd.Models.Algorithms;
using Msv.AutoMiner.FrontEnd.Models.EstimateProfitability;
using Msv.AutoMiner.FrontEnd.Models.Tools;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class EstimateProfitabilityController : Controller
    {
        private const string ElectricityCostUsdKey = "ElectricityCostUsd";

        private readonly ICoinNetworkInfoProvider m_NetworkInfoProvider;
        private readonly ICoinValueProvider m_CoinValueProvider;
        private readonly IRigHeartbeatProvider m_RigHeartbeatProvider;
        private readonly IProfitabilityTableBuilder m_TableBuilder;
        private readonly IOverallProfitabilityCalculator m_OverallProfitabilityCalculator;
        private readonly AutoMinerDbContext m_Context;

        public EstimateProfitabilityController(
            ICoinNetworkInfoProvider networkInfoProvider,
            ICoinValueProvider coinValueProvider,
            IRigHeartbeatProvider rigHeartbeatProvider,
            IProfitabilityTableBuilder tableBuilder,
            IOverallProfitabilityCalculator overallProfitabilityCalculator,
            AutoMinerDbContext context)
        {
            m_NetworkInfoProvider = networkInfoProvider;
            m_CoinValueProvider = coinValueProvider;
            m_RigHeartbeatProvider = rigHeartbeatProvider;
            m_TableBuilder = tableBuilder;
            m_OverallProfitabilityCalculator = overallProfitabilityCalculator;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var networkInfos = m_NetworkInfoProvider.GetCurrentNetworkInfos(false);
            var coinValues = m_CoinValueProvider.GetCurrentCoinValues(false);

            var rigNames = m_Context.Rigs.ToDictionary(x => x.Id, x => x.Name);
            var rigs = m_RigHeartbeatProvider.GetLastHeartbeats()
                .Join(rigNames, x => x.Key, x => x.Key, (x, y) => new RigModel
                {
                    Id = x.Key,
                    Name = y.Value,
                    HashRates = x.Value.heartbeat.AlgorithmMiningCapabilities.EmptyIfNull()
                })
                .OrderBy(x => x.Name)
                .Prepend(new RigModel
                {
                    Id = int.MinValue,
                    Name = "<overall>",
                    HashRates = m_OverallProfitabilityCalculator.CalculateTotalPower()
                        .Cast<AlgorithmPowerData>()
                        .ToArray()
                })
                .ToArray();
            return View(new EstimateProfitabilityIndexModel
            {
                ElectricityCostUsd = HttpContext.Session.GetDouble(ElectricityCostUsdKey),
                Rigs = rigs,
                Algorithms = m_Context.CoinAlgorithms
                    .Where(x => x.Activity == ActivityState.Active)
                    .Select(x => new AlgorithmBaseModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        KnownValue = x.KnownValue
                    })
                    .OrderBy(x => x.Name)
                    .ToArray(),
                Coins = m_Context.Coins
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .AsEnumerable()
                    .LeftOuterJoin(networkInfos, x => x.Id, x => x.CoinId, (x, y) => (coin:x, network: y))
                    .LeftOuterJoin(coinValues, x => x.coin.Id, x => x.CurrencyId,
                        (x, y) => (x.coin, x.network, price: y))
                    .Select(x => new CoinModel
                    {
                        Algorithm = new AlgorithmBaseModel {Id = x.coin.AlgorithmId},
                        BlockReward = x.network?.BlockReward,
                        Difficulty = x.network?.Difficulty,
                        Id = x.coin.Id,
                        Name = x.coin.Name,
                        Symbol = x.coin.Symbol,
                        MaxTarget = x.coin.MaxTarget,
                        BtcPrice = x.price?.AverageBtcValue
                    })
                    .OrderBy(x => x.Symbol)
                    .ToArray()
            });
        }

        [HttpPost]
        public IActionResult EstimateProfitability(EstimateProfitabilityRawRequestModel request)
        {
            var electricityCostUsd = ParsingHelper.ParseDouble(request.ElectricityCostUsd, true);
            if (electricityCostUsd > 0)
                HttpContext.Session.SetDouble(ElectricityCostUsdKey, electricityCostUsd);
            else
                HttpContext.Session.Remove(ElectricityCostUsdKey);

            return PartialView("_EstimatedProfitPartial", m_TableBuilder.EstimateProfitability(
                new EstimateProfitabilityRequest
                {
                    BlockReward = ParsingHelper.ParseDouble(request.BlockReward, true),
                    BtcPrice = ParsingHelper.ParseDouble(request.BtcPrice, true),
                    ClientHashRate = ParsingHelper.ParseHashRate(request.HashRate, true),
                    ClientPowerUsage = ParsingHelper.ParseDouble(request.ClientPowerUsage, true),
                    Difficulty = ParsingHelper.ParseDouble(request.Difficulty, true),
                    MaxTarget = request.MaxTarget,
                    ElectricityCostUsd = electricityCostUsd
                }));
        }
    }
}
