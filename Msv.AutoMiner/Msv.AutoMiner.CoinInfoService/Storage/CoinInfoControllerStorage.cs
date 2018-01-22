using System.Linq;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public class CoinInfoControllerStorage : ICoinInfoControllerStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public CoinInfoControllerStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public CoinAlgorithm[] GetAlgorithms()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.CoinAlgorithms.ToArray();
        }
    }
}
