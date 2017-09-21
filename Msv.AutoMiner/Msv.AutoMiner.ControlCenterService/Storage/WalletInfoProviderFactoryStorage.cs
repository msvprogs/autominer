using System.Linq;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.ControlCenterService.Storage.Contracts;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.Storage
{
    public class WalletInfoProviderFactoryStorage : IWalletInfoProviderFactoryStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public WalletInfoProviderFactoryStorage(AutoMinerDbContext context) 
            => m_Context = context;

        public Exchange GetExchange(ExchangeType type)
            => m_Context.Exchanges.AsNoTracking().FirstOrDefault(x => x.Type == type);
    }
}
