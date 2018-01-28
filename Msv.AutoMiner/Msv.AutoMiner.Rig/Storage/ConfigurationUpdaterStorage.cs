using System;
using System.Linq;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class ConfigurationUpdaterStorage : IConfigurationUpdaterStorage
    {
        public Miner[] GetMiners()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.Miners.AsNoTracking().ToArray();
        }

        public MinerAlgorithmSetting[] GetMinerAlgorithmSettings()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.MinerAlgorithmSettings
                    .Include(nameof(MinerAlgorithmSetting.Algorithm))
                    .AsNoTracking()
                    .ToArray();
        }

        public void SaveMiners(Miner[] miners)
        {
            if (miners == null) 
                throw new ArgumentNullException(nameof(miners));

            using (var context = new AutoMinerRigDbContext())
            {
                context.MinerAlgorithmSettings.RemoveRange(context.MinerAlgorithmSettings.ToArray());
                context.SaveChanges();

                context.Miners.RemoveRange(context.Miners.ToArray());
                context.SaveChanges();

                context.Miners.AddRange(miners);
                context.SaveChanges();
            }
        }

        public void SaveMinerAlgorithmSettings(MinerAlgorithmSetting[] algorithmSettings)
        {
            if (algorithmSettings == null) 
                throw new ArgumentNullException(nameof(algorithmSettings));

            using (var context = new AutoMinerRigDbContext())
            {
                context.MinerAlgorithmSettings.RemoveRange(context.MinerAlgorithmSettings.ToArray());
                context.SaveChanges();

                context.MinerAlgorithmSettings.AddRange(algorithmSettings);
                context.SaveChanges();
            }
        }

        public void SaveAlgorithms(AlgorithmData[] algorithmDatas)
        {
            if (algorithmDatas == null) 
                throw new ArgumentNullException(nameof(algorithmDatas));

            using (var context = new AutoMinerRigDbContext())
            {
                var existingAlgorithms = context.AlgorithmDatas.ToArray();
                var newAlgorithms = algorithmDatas.LeftOuterJoin(existingAlgorithms,
                        x => x.AlgorithmId, x => x.AlgorithmId, (x, y) => y == null ? x : null)
                    .Where(x => x != null)
                    .ToArray();
                existingAlgorithms.Join(algorithmDatas, x => x.AlgorithmId, x => x.AlgorithmId, 
                    (x, y) => (existing:x, newOne: y))
                    .ForEach(x => x.existing.AlgorithmName = x.newOne.AlgorithmName);
                context.AlgorithmDatas.AddRange(newAlgorithms);
                context.SaveChanges();
            }
        }
    }
}
