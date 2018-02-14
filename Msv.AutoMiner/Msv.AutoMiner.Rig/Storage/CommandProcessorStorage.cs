using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class CommandProcessorStorage : ICommandProcessorStorage
    {
        public AlgorithmData[] GetAlgorithms()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.AlgorithmDatas.AsNoTracking().ToArray();
        }

        public void StoreAlgorithms(AlgorithmData[] newAlgorithms)
        {
            if (newAlgorithms == null)
                throw new ArgumentNullException(nameof(newAlgorithms));

            using (var context = new AutoMinerRigDbContext())
            {
                context.AlgorithmDatas.AddRange(newAlgorithms);
                context.SaveChanges();
            }
        }

        public Miner[] GetMiners()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.Miners.AsNoTracking().ToArray();
        }

        public void SaveMiner(Miner miner)
        {
            using (var context = new AutoMinerRigDbContext())
            {
                var existing = context.Miners.First(x => x.Id == miner.Id);
                existing.FileName = miner.FileName;
                existing.SecondaryFileName = miner.SecondaryFileName;
                context.SaveChanges();
            }
        }

        public MinerAlgorithmSetting[] GetMinerAlgorithmSettings()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.MinerAlgorithmSettings
                    .Include(x => x.Miner)
                    .AsNoTracking()
                    .ToArray();
        }

        public MinerAlgorithmSetting GetMinerAlgorithmSetting(string algorithmName)
        {
            if (algorithmName == null)
                throw new ArgumentNullException(nameof(algorithmName));

            using (var context = new AutoMinerRigDbContext())
            {
                var algorithm = context.AlgorithmDatas.FirstOrDefault(x => x.AlgorithmName == algorithmName);
                if (algorithm == null)
                    return null;
                return context.MinerAlgorithmSettings
                           .FirstOrDefault(x => x.AlgorithmId == algorithm.AlgorithmId) 
                           ?? new MinerAlgorithmSetting {AlgorithmId = algorithm.AlgorithmId};
            }
        }

        public void SaveMinerAlgorithmSetting(MinerAlgorithmSetting setting)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            using (var context = new AutoMinerRigDbContext())
            {
                var entity = context.MinerAlgorithmSettings
                                 .FirstOrDefault(x => x.AlgorithmId == setting.AlgorithmId)
                             ?? context.MinerAlgorithmSettings.Add(setting);
                entity.MinerId = setting.MinerId;
                entity.AlgorithmArgument = setting.AlgorithmArgument;
                entity.Intensity = setting.Intensity.NullIfNaN();
                entity.LogFile = setting.LogFile;
                entity.AdditionalArguments = setting.AdditionalArguments;
                context.SaveChanges();
            }
        }

        public ManualDeviceMapping[] GetManualMappings()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.ManualDeviceMappings.AsNoTracking().ToArray();
        }

        public void SaveManualMappings(ManualDeviceMapping[] mappings)
        {
            if (mappings == null)
                throw new ArgumentNullException(nameof(mappings));

            using (var context = new AutoMinerRigDbContext())
            {
                var existingMappings = context.ManualDeviceMappings.AsNoTracking().ToArray();
                context.ManualDeviceMappings.RemoveRange(
                    existingMappings.Join(mappings, x => new {x.DeviceType, x.DeviceId},
                            x => new {x.DeviceType, x.DeviceId}, (x, y) => y)
                        .ToArray());
                context.SaveChanges();
                context.ManualDeviceMappings.AddRange(mappings);
                context.SaveChanges();
            }
        }

        public void ClearManualMappings(KeyValuePair<DeviceType, int>[] deviceIds)
        {
            if (deviceIds == null)
                throw new ArgumentNullException(nameof(deviceIds));

            using (var context = new AutoMinerRigDbContext())
            {
                var existingMappings = context.ManualDeviceMappings.AsNoTracking().ToArray();
                context.ManualDeviceMappings.RemoveRange(
                    existingMappings.Join(deviceIds, x => new { x.DeviceType, x.DeviceId },
                            x => new { DeviceType = x.Key, DeviceId = x.Value }, (x, y) => x)
                        .ToArray());
                context.SaveChanges();
            }
        }
    }
}
