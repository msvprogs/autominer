using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Data.Logic
{
    public class RigHeartbeatProvider : IRigHeartbeatProvider
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public RigHeartbeatProvider(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public (Heartbeat heartbeat, RigHeartbeat entity) GetLastHeartbeat(int rigId)
        {
            using (var context = m_Factory.Create())
            {
                var entity = context.RigHeartbeats
                    .OrderByDescending(x => x.Received)
                    .FirstOrDefault(x => x.RigId == rigId);
                return entity != null 
                    ? (JsonConvert.DeserializeObject<Heartbeat>(entity.ContentsJson), entity)
                    : default;
            }
        }

        public Dictionary<int, (Heartbeat heartbeat, RigHeartbeat entity)> GetLastHeartbeats(int[] rigIds = null)
        {
            var sqlQuery = $@"SELECT source.* FROM RigHeartbeats source
  JOIN (SELECT RigId, MAX(Received) AS MaxReceived FROM RigHeartbeats
    GROUP BY RigId) as grouped
  ON source.RigId = grouped.RigId AND source.Received = grouped.MaxReceived
  {(rigIds != null && rigIds.Any() ? "WHERE source.RigId IN (" + string.Join(",", rigIds) + ")" : "")} ;";

            using (var context = m_Factory.Create())
            {
                return context.RigHeartbeats
                    .FromSql(sqlQuery)
                    .AsEnumerable()
                    .GroupBy(x => x.RigId)
                    .Select(x => new
                    {
                        x.Key,
                        LastHeartbeat = x.OrderByDescending(y => y.Received).First()
                    })
                    .ToDictionary(
                        x => x.Key,
                        x => (JsonConvert.DeserializeObject<Heartbeat>(x.LastHeartbeat.ContentsJson), x.LastHeartbeat));
            }
        }
    }
}
