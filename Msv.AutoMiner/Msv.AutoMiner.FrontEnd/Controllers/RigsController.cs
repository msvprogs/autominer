using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Infrastructure;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.FrontEnd.Infrastructure.Contracts;
using Msv.AutoMiner.FrontEnd.Models.Rigs;
using Newtonsoft.Json;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class RigsController : EntityControllerBase<Rig, RigDisplayModel, int>
    {
        public const string RigsMessageKey = "RigsMessage";

        private readonly IRigHeartbeatProvider m_HeartbeatProvider;
        private readonly ICryptoRandomGenerator m_RandomGenerator;
        private readonly AutoMinerDbContext m_Context;

        public RigsController(
            IRigHeartbeatProvider heartbeatProvider, 
            ICryptoRandomGenerator randomGenerator,
            AutoMinerDbContext context)
            : base("_RigRowPartial", context)
        {
            m_HeartbeatProvider = heartbeatProvider;
            m_RandomGenerator = randomGenerator;
            m_Context = context;
        }

        public IActionResult Index() 
            => View(GetEntityModels(null));

        [HttpPost]
        public async Task<IActionResult> CreateRegistrationRequest(int id)
        {
            var rig = await m_Context.Rigs
                .Where(x => x.Activity == ActivityState.Active)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (rig == null)
                return NotFound();

            rig.RegistrationPassword = BitConverter.ToUInt32(m_RandomGenerator.GenerateRandom(4), 0).ToString();
            await m_Context.SaveChangesAsync();
            return Content($"Rig name: <b>{rig.Name}</b><br />Password for rig registration: <b>{rig.RegistrationPassword}</b>");
        }

        [HttpPost]
        public async Task<IActionResult> RevokeCertificate(int id)
        {
            var rig = await m_Context.Rigs.FirstOrDefaultAsync(x => x.Id == id);
            if (rig == null)
                return NotFound();
            rig.ClientCertificateSerial = null;
            rig.ClientCertificateThumbprint = null;

            await m_Context.SaveChangesAsync();
            return PartialView("_RigRowPartial", GetEntityModels(new[] {id}).FirstOrDefault());
        }

        public IActionResult Create()
            => View("Edit", new RigEditModel
            {
                DifficultyAggregationType = ValueAggregationType.Last12Hours,
                PriceAggregationType = ValueAggregationType.Last24Hours
            });

        public async Task<IActionResult> Edit(int id)
        {
            var rig = await m_Context.Rigs.FirstOrDefaultAsync(x => x.Id == id);
            if (rig == null)
                return NotFound();
            return View(new RigEditModel
            {
                Id = id,
                DifficultyAggregationType = rig.DifficultyAggregationType,
                Name = rig.Name,
                PriceAggregationType = rig.PriceAggregationType
            });
        }

        [HttpPost]
        public async Task<IActionResult> Save(RigEditModel model)
        {
            if (!ModelState.IsValid)
                return View("Edit", model);

            var rig = await m_Context.Rigs.FirstOrDefaultAsync(x => x.Id == model.Id)
                      ?? m_Context.Rigs.Add(new Rig
                      {
                          Activity = ActivityState.Active
                      }).Entity;
            rig.Name = model.Name;
            rig.DifficultyAggregationType = model.DifficultyAggregationType;
            rig.PriceAggregationType = model.PriceAggregationType;

            await m_Context.SaveChangesAsync();
            TempData[RigsMessageKey] = $"Rig {rig.Name} has been successfully saved";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Statistics(int id)
        {
            var rig = await m_Context.Rigs.FirstOrDefaultAsync(x => x.Id == id);
            if (rig == null)
                return NotFound();

            var heartbeatTimeLimit = DateTime.UtcNow.Date.AddDays(-1);
            var lastDayHeartbeats = (await m_Context.RigHeartbeats
                    .Where(x => x.RigId == id && x.Received >= heartbeatTimeLimit)
                    .ToArrayAsync())
                .Select(x => JsonConvert.DeserializeObject<Heartbeat>(x.ContentsJson))
                .Where(x => x != null)
                .ToArray();

            var lastProfitability = await m_Context.CoinProfitabilities
                .Where(x => x.RigId == id)
                .OrderByDescending(x => x.Requested)
                .FirstOrDefaultAsync();
            var lastProfitabilityTime = lastProfitability?.Requested ?? DateTime.MinValue;
            var profitabilityTable = await m_Context.CoinProfitabilities
                .Include(x => x.Coin)
                .Include(x => x.Pool)
                .Where(x => x.RigId == id && x.Requested == lastProfitabilityTime)
                .Select(x => new RigStatisticsModel.CoinProfitabilityInfo
                {
                    UsdPerDay = x.UsdPerDay - x.ElectricityCost,
                    BtcPerDay = x.BtcPerDay,
                    CoinsPerDay = x.CoinsPerDay,
                    CoinId = x.CoinId,
                    CoinName = x.Coin.Name,
                    Logo = x.Coin.LogoImageBytes,
                    PoolName = x.Pool.Name,
                    PoolId = x.PoolId
                })
                .ToArrayAsync();

            var coins = await m_Context.Coins.ToDictionaryAsync(x => x.Id);
            var durations = new List<RigStatisticsModel.CoinMiningDuration>();
            Heartbeat.MiningState currentMiningState = null;
            foreach (var heartbeat in lastDayHeartbeats.OrderBy(x => x.DateTime))
            {
                var heartbeatState = heartbeat.MiningStates?.FirstOrDefault();
                if (heartbeatState == null)
                {
                    currentMiningState = null;
                    continue;
                }

                if (currentMiningState != null && currentMiningState.CoinId != heartbeatState.CoinId)
                {
                    durations.Add(new RigStatisticsModel.CoinMiningDuration
                    {
                        CoinName = coins[currentMiningState.CoinId].Name,
                        CoinSymbol = coins[currentMiningState.CoinId].Symbol,
                        Duration = currentMiningState.Duration,
                        Time = heartbeat.DateTime
                    });
                }
                currentMiningState = heartbeatState;
            }

            var lastRigHeartbeat = m_HeartbeatProvider.GetLastHeartbeat(id);
            if (currentMiningState != null && lastRigHeartbeat.entity != null)
                durations.Add(new RigStatisticsModel.CoinMiningDuration
                {
                    CoinName = coins[currentMiningState.CoinId].Name,
                    CoinSymbol = coins[currentMiningState.CoinId].Symbol,
                    Duration = currentMiningState.Duration,
                    Time = lastRigHeartbeat.entity.Received
                });

            return View(new RigStatisticsModel
            {
                Id = id,
                Name = rig.Name,
                LastHeartbeat = lastRigHeartbeat.heartbeat,
                LastProfitabilityTable = profitabilityTable,
                ProfitabilityTableTime = lastProfitabilityTime != default
                    ? lastProfitabilityTime
                    : (DateTime?) null,
                LastDayActivity = durations.ToArray(),
                Algorithms = m_Context.CoinAlgorithms
                    .AsNoTracking()
                    .Where(x => x.Activity == ActivityState.Active)
                    .ToArray()
            });
        }

        protected override RigDisplayModel[] GetEntityModels(int[] ids)
        {
            var lastHeartbeats = m_HeartbeatProvider.GetLastHeartbeats();
            var rigQuery = m_Context.Rigs
                .Where(x => x.Activity != ActivityState.Deleted)
                .AsNoTracking();
            if (!ids.IsNullOrEmpty())
                rigQuery = rigQuery.Where(x => ids.Contains(x.Id));
            return rigQuery
                .AsEnumerable()
                .LeftOuterJoin(lastHeartbeats, x => x.Id, x => x.Key,
                    (x, y) => (rig: x, 
                        heartbeat: y.Value.heartbeat ?? new Heartbeat(),
                        heartbeatEntity: y.Value.entity ?? new RigHeartbeat()))
                .Select(x => new RigDisplayModel
                {
                    Id = x.rig.Id,
                    Name = x.rig.Name,
                    Activity = x.rig.Activity,
                    RemoteAddress = x.heartbeatEntity.RemoteAddress,
                    DifficultyAggregationType = x.rig.DifficultyAggregationType,
                    PriceAggregationType = x.rig.PriceAggregationType,
                    CertificateSerial = x.rig.ClientCertificateSerial != null
                        ? HexHelper.ToHex(x.rig.ClientCertificateSerial)
                        : null,
                    LastHeartbeat = x.heartbeatEntity.Received != default
                        ? x.heartbeatEntity.Received
                        : (DateTime?)null
                })
                .ToArray();
        }
    }
}
