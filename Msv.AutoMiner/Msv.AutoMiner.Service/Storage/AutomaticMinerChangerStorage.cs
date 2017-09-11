using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Storage.Contracts;

namespace Msv.AutoMiner.Service.Storage
{
    public class AutomaticMinerChangerStorage : IAutomaticMinerChangerStorage
    {
        public void SaveProfitabilities(CoinProfitability[] profitabilities)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.CoinProfitabilities.AddRange(profitabilities);
                context.SaveChanges();
            }
        }

        public void SaveChangeEvents(MiningChangeEvent[] miningChangeEvent)
        {
            using (var context = new AutoMinerDbContext())
            {
                context.MiningChangeEvents.AddRange(miningChangeEvent);
                context.SaveChanges();
            }
        }
    }
}
