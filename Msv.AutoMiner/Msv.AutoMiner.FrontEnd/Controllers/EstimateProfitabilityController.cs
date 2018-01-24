﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.ServiceContracts;
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
        private readonly ICoinInfoService m_CoinInfoService;
        private readonly AutoMinerDbContext m_Context;

        public EstimateProfitabilityController(
            ICoinNetworkInfoProvider networkInfoProvider,
            ICoinValueProvider coinValueProvider,
            IRigHeartbeatProvider rigHeartbeatProvider,
            ICoinInfoService coinInfoService,
            AutoMinerDbContext context)
        {
            m_NetworkInfoProvider = networkInfoProvider;
            m_CoinValueProvider = coinValueProvider;
            m_RigHeartbeatProvider = rigHeartbeatProvider;
            m_CoinInfoService = coinInfoService;
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
                .ToArray();
            return View(new EstimateProfitabilityIndexModel
            {
                ElectricityCostUsd = HttpContext.Session.GetDouble(ElectricityCostUsdKey),
                Rigs = rigs,
                Algorithms = m_Context.CoinAlgorithms
                    .Select(x => new AlgorithmModel
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
                        Algorithm = new AlgorithmModel {Id = x.coin.AlgorithmId},
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
        public async Task<IActionResult> EstimateProfitability(EstimateProfitabilityRawRequestModel request)
        {
            var electricityCostUsd = ParsingHelper.ParseDouble(request.ElectricityCostUsd, true);
            if (electricityCostUsd > 0)
                HttpContext.Session.SetDouble(ElectricityCostUsdKey, electricityCostUsd);
            else
                HttpContext.Session.Remove(ElectricityCostUsdKey);

            return PartialView("_EstimatedProfitPartial", await m_CoinInfoService.EstimateProfitability(
                new EstimateProfitabilityRequestModel
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