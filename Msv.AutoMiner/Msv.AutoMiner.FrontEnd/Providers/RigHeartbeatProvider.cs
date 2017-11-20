using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public class RigHeartbeatProvider : IRigHeartbeatProvider
    {
        private static readonly TimeSpan M_OldestHeartbeatInterval = TimeSpan.FromDays(3);

        private readonly AutoMinerDbContext m_Context;

        public RigHeartbeatProvider(AutoMinerDbContext context)
            => m_Context = context;

        public Heartbeat GetLastHeartbeat(int rigId)
        {
            var entity = m_Context.RigHeartbeats
                .OrderByDescending(x => x.Received)
                .FirstOrDefault(x => x.RigId == rigId);
            return entity != null 
                ? JsonConvert.DeserializeObject<Heartbeat>(entity.ContentsJson)
                : null;
        }

        public Dictionary<int, Heartbeat> GetLastActiveHeartbeats()
        {
            var rigIds = m_Context.Rigs
                .Where(x => x.IsActive)
                .Select(x => x.Id)
                .ToArray();
            var maxDates = GetMaxDates(rigIds).Values.ToArray();
            return m_Context.RigHeartbeats
                .Where(x => rigIds.Contains(x.RigId) && maxDates.Contains(x.Received))
                .AsEnumerable()
                .GroupBy(x => x.RigId)
                .ToDictionary(
                    x => x.Key,
                    x => JsonConvert.DeserializeObject<Heartbeat>(
                        x.OrderByDescending(y => y.Received)
                            .First()
                            .ContentsJson));
        }

        private Dictionary<int, DateTime> GetMaxDates(int[] rigIds)
        {
            var minDate = DateTime.UtcNow - M_OldestHeartbeatInterval;
            var lastDates = m_Context.RigHeartbeats
                .AsNoTracking()
                .Where(x => x.Received > minDate && rigIds.Contains(x.RigId))
                .Select(x => new {x.RigId, x.Received})
                .ToArray();
            return lastDates.GroupBy(x => x.RigId)
                .ToDictionary(x => x.Key, x => x.OrderByDescending(y => y.Received).First().Received);
        }
    }
}
