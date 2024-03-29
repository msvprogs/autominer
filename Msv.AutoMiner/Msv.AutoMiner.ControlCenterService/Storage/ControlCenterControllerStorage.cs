﻿using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class ControlCenterControllerStorage : IControlCenterControllerStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public ControlCenterControllerStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Rig GetRigByName(string name)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Rigs.FirstOrDefault(x => x.Name == name);
        }

        public Rig GetRigById(int rigId)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Rigs.First(x => x.Id == rigId);
        }

        public void SaveRig(Rig rig)
        {
            if (rig == null)
                throw new ArgumentNullException(nameof(rig));

            using (var context = m_Factory.Create())
            {
                var existingRig = context.Rigs.First(x => x.Id == rig.Id);
                existingRig.ClientCertificateSerial = rig.ClientCertificateSerial;
                existingRig.ClientCertificateThumbprint = rig.ClientCertificateThumbprint;
                existingRig.RegistrationPassword = rig.RegistrationPassword;
                context.SaveChanges();
            }
        }

        public void SaveHeartbeat(RigHeartbeat heartbeat)
        {
            if (heartbeat == null)
                throw new ArgumentNullException(nameof(heartbeat));

            using (var context = m_Factory.Create())
            {
                context.RigHeartbeats.Add(heartbeat);
                context.SaveChanges();
            }
        }

        public void SaveMiningStates(RigMiningState[] miningStates)
        {
            if (miningStates == null)
                throw new ArgumentNullException(nameof(miningStates));

            using (var context = m_Factory.Create())
            {
                context.RigMiningStates.AddRange(miningStates);
                context.SaveChanges();
            }
        }

        public RigCommand GetNextCommand(int rigId)
        {
            using (var context = m_Factory.CreateReadOnly())
            {
                return context.RigCommands
                    .Where(x => x.RigId == rigId && x.Sent == null)
                    .OrderBy(x => x.Created)
                    .FirstOrDefault();
            }
        }

        public void MarkCommandAsSent(int commandId)
        {
            using (var context = m_Factory.Create())
            {
                var command = context.RigCommands.First(x => x.Id == commandId);
                command.Sent = DateTime.UtcNow;
                context.SaveChanges();
            }
        }

        public void SaveProfitabilities(CoinProfitability[] profitabilities)
        {
            if (profitabilities == null)
                throw new ArgumentNullException(nameof(profitabilities));

            using (var context = m_Factory.Create())
            {
                context.CoinProfitabilities.AddRange(profitabilities);
                context.SaveChanges();
            }
        }

        public MinerVersion[] GetLastMinerVersions(PlatformType platform)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.MinerVersions
                    .Include(x => x.Miner)
                    .FromSql(@"SELECT ver.* FROM MinerVersions ver
  join (select MinerId, Platform, MAX(Uploaded) as MaxUploaded
    from MinerVersions
    group by MinerId, Platform) maxUploaded
  on ver.MinerId = maxUploaded.MinerId and ver.Platform = maxUploaded.Platform and ver.Uploaded = maxUploaded.MaxUploaded")
                    .Where(x => x.Platform == platform)
                    .Where(x => x.Miner.Activity == ActivityState.Active)
                    .ToArray();
        }

        public CoinAlgorithm[] GetAlgorithms()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.CoinAlgorithms
                        .Where(x => x.Activity == ActivityState.Active)
                        .ToArray();
        }
    }
}
