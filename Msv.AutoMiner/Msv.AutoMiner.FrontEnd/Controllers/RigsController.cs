using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.FrontEnd.Models.Rigs;

namespace Msv.AutoMiner.FrontEnd.Controllers
{
    public class RigsController : Controller
    {
        private readonly AutoMinerDbContext m_Context;

        public RigsController(AutoMinerDbContext context)
            => m_Context = context;

        public async Task<IActionResult> Index()
        {
            var lastHeartbeats = await m_Context.RigHeartbeats
                .GroupBy(x => x.RigId)
                .Select(x => new
                {
                    RigId = x.Key,
                    Heartbeat = x.OrderByDescending(y => y.Received).FirstOrDefault()
                })
                .ToDictionaryAsync(x => x.RigId, x => x.Heartbeat);

            var rigs = m_Context.Rigs
                .AsNoTracking()
                .AsEnumerable()
                .LeftOuterJoin(lastHeartbeats, x => x.Id, x => x.Key,
                    (x, y) => (rig: x, heartbeat: y.Value ?? new RigHeartbeat()))
                .Select(x => new RigDisplayModel
                {
                    Id = x.rig.Id,
                    Name = x.rig.Name,
                    IsActive = x.rig.IsActive,
                    CertificateSerial = x.rig.ClientCertificateSerial != null
                        ? HexHelper.ToHex(x.rig.ClientCertificateSerial)
                        : null,
                    LastHeartbeat = x.heartbeat.Received != default(DateTime)
                        ? x.heartbeat.Received
                        : (DateTime?)null
                })
                .ToArray();

            return View(rigs);
        }
    }
}
