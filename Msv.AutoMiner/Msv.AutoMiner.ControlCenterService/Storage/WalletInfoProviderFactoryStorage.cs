using System.Linq;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class WalletInfoProviderFactoryStorage : IWalletInfoProviderFactoryStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public WalletInfoProviderFactoryStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Exchange GetExchange(ExchangeType type)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Exchanges.FirstOrDefault(x => x.Type == type);
        }
    }
}
