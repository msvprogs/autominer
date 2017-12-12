using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Data.Logic
{
    public class RigHeartbeatProvider : IRigHeartbeatProvider
    {
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

        public Dictionary<int, Heartbeat> GetLastHeartbeats(int[] rigIds = null)
        {
            var sqlQuery = $@"SELECT source.* FROM RigHeartbeats source
  JOIN (SELECT RigId, MAX(Received) AS MaxReceived FROM RigHeartbeats
    GROUP BY RigId) as grouped
  ON source.RigId = grouped.RigId AND source.Received = grouped.MaxReceived
  {(rigIds != null && rigIds.Any() ? "WHERE source.RigId IN (" + string.Join(",", rigIds) + ")" : "")} ;";

            return m_Context.RigHeartbeats
                .FromSql(sqlQuery)
                .AsEnumerable()
                .GroupBy(x => x.RigId)
                .ToDictionary(
                    x => x.Key,
                    x => JsonConvert.DeserializeObject<Heartbeat>(
                        x.OrderByDescending(y => y.Received)
                            .First()
                            .ContentsJson));
        }
    }
}
