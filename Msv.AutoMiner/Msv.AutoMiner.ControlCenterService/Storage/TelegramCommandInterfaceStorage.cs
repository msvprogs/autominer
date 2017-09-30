using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Newtonsoft.Json;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class TelegramCommandInterfaceStorage : ITelegramCommandInterfaceStorage
    {
        private readonly string m_ConnectionString;

        public TelegramCommandInterfaceStorage(string connectionString)
        {
            m_ConnectionString = connectionString;
        }

        public void StoreTelegramUser(TelegramUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                var existing = context.TelegramUsers.FirstOrDefault(x => x.UserName == user.UserName);
                if (existing != null)
                    return;
                context.TelegramUsers.Add(user);
                context.SaveChanges();
            }
        }

        public KeyValuePair<string, Heartbeat>[] GetLastHeartbeats(string[] rigNames)
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
            {
                var heartbeats = context.RigHeartbeats
                    .GroupBy(x => x.RigId)
                    .Select(x => new
                    {
                        x.Key,
                        LastHeartbeat = x.OrderByDescending(y => y.Received).FirstOrDefault()
                    })
                    .ToDictionary(x => x.Key, x => x.LastHeartbeat);
                IQueryable<Rig> rigs = context.Rigs;
                if (rigNames != null)
                    rigs = rigs.Where(x => rigNames.Contains(x.Name.ToLower()));
                return rigs
                    .Where(x => x.IsActive)
                    .Join(heartbeats, x => x.Id, x => x.Key,
                        (x, y) => new KeyValuePair<string, Heartbeat>(
                            x.Name, JsonConvert.DeserializeObject<Heartbeat>(y.Value.ContentsJson)))
                    .ToArray();
            }
        }

        public Coin[] GetCoins()
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.Coins
                    .Include(x => x.Algorithm)
                    .ToArray();
        }
    }
}
