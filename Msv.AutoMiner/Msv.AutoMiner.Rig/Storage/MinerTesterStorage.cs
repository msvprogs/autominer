using System;
using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Rig.Storage.Contracts;
using Msv.AutoMiner.Rig.Storage.Model;

namespace Msv.AutoMiner.Rig.Storage
{
    public class MinerTesterStorage : IMinerTesterStorage
    {
        public MinerAlgorithmSetting[] GetMinerAlgorithmSettings()
        {
            using (var context = new AutoMinerRigDbContext())
                return context.MinerAlgorithmSettings
                    .Include(x => x.Miner)
                    .AsNoTracking()
                    .ToArray();
        }

        public void StoreAlgorithmData(Guid algorithmId, long hashRate, double power)
        {
            using (var context = new AutoMinerRigDbContext())
            {
                var data = context.AlgorithmDatas.First(x => x.AlgorithmId == algorithmId);
                data.SpeedInHashes = hashRate;
                data.Power = power;
                context.SaveChanges();
            }
        }
    }
}
