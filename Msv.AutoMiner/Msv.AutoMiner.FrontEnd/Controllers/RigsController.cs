using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Rigs;
using Msv.AutoMiner.FrontEnd.Models.Shared;
using Msv.AutoMiner.FrontEnd.Providers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class RigsController : Controller
    {
        public const string RigsMessageKey = "RigsMessage";

        private readonly IRigHeartbeatProvider m_HeartbeatProvider;
        private readonly AutoMinerDbContext m_Context;

        public RigsController(IRigHeartbeatProvider heartbeatProvider, AutoMinerDbContext context)
        {
            m_HeartbeatProvider = heartbeatProvider;
            m_Context = context;
        }

        public IActionResult Index()
        {
            var lastHeartbeats = m_HeartbeatProvider.GetLastHeartbeats();
            var rigs = m_Context.Rigs
                .AsNoTracking()
                .AsEnumerable()
                .LeftOuterJoin(lastHeartbeats, x => x.Id, x => x.Key,
                    (x, y) => (rig: x, heartbeat: y.Value ?? new Heartbeat()))
                .Select(x => new RigDisplayModel
                {
                    Id = x.rig.Id,
                    Name = x.rig.Name,
                    IsActive = x.rig.IsActive,
                    CertificateSerial = x.rig.ClientCertificateSerial != null
                        ? HexHelper.ToHex(x.rig.ClientCertificateSerial)
                        : null,
                    LastHeartbeat = x.heartbeat.DateTime != default
                        ? x.heartbeat.DateTime
                        : (DateTime?)null
                })
                .ToArray();
            return View(rigs);
        }

        public async Task<IActionResult> CreateRegistrationRequest(int id)
        {
            var rig = await m_Context.Rigs.FirstOrDefaultAsync(x => x.Id == id);
            if (rig == null)
                return NotFound();
            using (var prng = new RNGCryptoServiceProvider())
            {
                var passwordBytes = new byte[4];
                prng.GetBytes(passwordBytes);
                rig.RegistrationPassword = BitConverter.ToUInt32(passwordBytes, 0).ToString();
            }
            await m_Context.SaveChangesAsync();
            return PartialView("_AlertPartial", new AlertModel
            {
                Type = AlertType.Info,
                Body = $"Rig name: {rig.Name}, password for rig registration: {rig.RegistrationPassword}",
                Title = $"Registration of rig {rig.Name}"
            });
        }

        [HttpGet, HttpPost]
        public async Task<IActionResult> RevokeCertificate(int id)
        {
            var rig = await m_Context.Rigs.FirstOrDefaultAsync(x => x.Id == id);
            if (rig == null)
                return NotFound();
            if (HttpContext.Request.Method == HttpMethods.Get)
                return PartialView("_ConfirmPartial", new ConfirmModel
                {
                    Title = $"Revoke certificate of rig {rig.Name}",
                    Body = $"Would you really like to revoke certificate of rig {rig.Name}?" 
                        + " It will no longer be able to connect to the control center."
                });

            if (HttpContext.Request.Method != HttpMethods.Post)
                throw new InvalidOperationException();
            rig.ClientCertificateSerial = null;
            rig.ClientCertificateThumbprint = null;
            await m_Context.SaveChangesAsync();
            TempData[RigsMessageKey] = $"Certificate of rig {rig.Name} was revoked";
            return RedirectToAction("Index");
        }

        public IActionResult Edit()
        {
            throw new NotImplementedException();
        }

        public IActionResult ToggleActive()
        {
            throw new NotImplementedException();
        }

        public IActionResult Create()
        {
            throw new NotImplementedException();
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

            var lastProfitabilityTime = await m_Context.CoinProfitabilities
                .Where(x => x.RigId == id)
                .Select(x => x.Requested)
                .DefaultIfEmpty(default)
                .MaxAsync();
            var profitabilityTable = await m_Context.CoinProfitabilities
                .Include(x => x.Coin)
                .Include(x => x.Pool)
                .Where(x => x.RigId == id && x.Requested == lastProfitabilityTime)
                .Select(x => new RigStatisticsModel.CoinProfitabilityInfo
                {
                    UsdPerDay = x.UsdPerDay - x.ElectricityCost,
                    BtcPerDay = x.BtcPerDay,
                    CoinId = x.CoinId,
                    CoinName = x.Coin.Name,
                    CoinSymbol = x.Coin.Symbol,
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
            if (currentMiningState != null && lastRigHeartbeat != null)
                durations.Add(new RigStatisticsModel.CoinMiningDuration
                {
                    CoinName = coins[currentMiningState.CoinId].Name,
                    CoinSymbol = coins[currentMiningState.CoinId].Symbol,
                    Duration = currentMiningState.Duration,
                    Time = lastRigHeartbeat.DateTime
                });

            return View(new RigStatisticsModel
            {
                Id = id,
                Name = rig.Name,
                LastHeartbeat = lastRigHeartbeat,
                LastProfitabilityTable = profitabilityTable,
                ProfitabilityTableTime = lastProfitabilityTime != default
                    ? lastProfitabilityTime
                    : (DateTime?) null,
                LastDayActivity = durations.ToArray(),
                Algorithms = m_Context.CoinAlgorithms.AsNoTracking().ToArray()
            });
        }
    }
}
