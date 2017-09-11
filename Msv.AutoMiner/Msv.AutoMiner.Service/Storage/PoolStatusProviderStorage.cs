using System.Data.Entity;
using System.Linq;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class PoolStatusProviderStorage : IPoolStatusProviderStorage
    {
        public Pool GetPool(int poolId)
        {
            using (var context = new AutoMinerDbContext())
                return context.Pools
                    .Include(x => x.Coins)
                    .First(x => x.Id == poolId);
        }

        public void SavePool(Pool pool)
        {
            using (var context = new AutoMinerDbContext())
            {
                var existingPool = context.Pools.First(x => x.Id == pool.Id);
                existingPool.ResponsesStoppedDate = pool.ResponsesStoppedDate;
                context.SaveChanges();
            }
        }
    }
}
