﻿using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Logic.Storage
{
    public class RigStatusNotifierStorage : IRigStatusNotifierStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public RigStatusNotifierStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Rig GetRig(int rigId)
        {
            using (var context = m_Factory.Create())
                return context.Rigs.AsNoTracking().First(x => x.Id == rigId);
        }

        public int[] GetReceiverIds(string[] userWhiteList)
        {
            using (var context = m_Factory.Create())
                return context.TelegramUsers
                    .Where(x => userWhiteList.Contains(x.UserName))
                    .Select(x => x.Id)
                    .ToArray();
        }
    }
}
