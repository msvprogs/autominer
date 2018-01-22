using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class TelegramCommandInterfaceStorage : ITelegramCommandInterfaceStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public TelegramCommandInterfaceStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public void StoreTelegramUser(TelegramUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using (var context = m_Factory.Create())
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
            using (var context = m_Factory.CreateReadOnly())
                return context.Rigs
                    .Where(x => ids.Contains(x.Id))
                    .ToDictionary(x => x.Id, x => x.Name);
        }

        public int[] GetRigIds(string[] names)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Rigs
                    .Where(x => names.Contains(x.Name))
                    .Select(x => x.Id)
                    .ToArray();
        }

        public Coin[] GetCoins()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Coins
                    .Include(x => x.Algorithm)
                    .ToArray();
        }
    }
}
