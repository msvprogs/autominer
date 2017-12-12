using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;

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

        public Dictionary<int, string> GetRigNames(int[] ids)
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.Rigs
                    .AsNoTracking()
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionary(x => x.Id, x => x.Name);
        }

        public int[] GetRigIds(string[] names)
        {
            using (var context = new AutoMinerDbContext(m_ConnectionString))
                return context.Rigs
                    .AsNoTracking()
                    .Where(x => names.Contains(x.Name))
                    .Select(x => x.Id)
                    .ToArray();
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
