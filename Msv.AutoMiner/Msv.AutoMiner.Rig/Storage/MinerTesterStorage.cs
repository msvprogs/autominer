using System;
using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Common;
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
                    .Include(x => x.Algorithm)
                    .AsNoTracking()
                    .ToArray();
        }

        public void StoreAlgorithmData(Guid algorithmId, string algorithmName, double hashRate, double power)
        {
            using (var context = new AutoMinerRigDbContext())
            {
                var idString = algorithmId.ToString();
                var data = context.AlgorithmDatas.FirstOrDefault(x => x.AlgorithmId == idString)
                           ?? context.AlgorithmDatas.Add(new AlgorithmData
                           {
                               AlgorithmId = idString,
                               AlgorithmName = algorithmName
                           });
                data.SpeedInHashes = hashRate.ZeroIfNaN();
                data.Power = power.ZeroIfNaN();
                context.SaveChanges();
            }
        }
    }
}
