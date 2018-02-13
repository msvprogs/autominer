using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class FiatValueMonitorStorage : IFiatValueMonitorStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public FiatValueMonitorStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public Coin[] GetCoins()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.Coins
                    .Where(x => x.Activity != ActivityState.Deleted)
                    .ToArray();
        }

        public FiatCurrency[] GetFiatCurrencies()
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.FiatCurrencies.ToArray();
        }

        public void StoreValues(CoinFiatValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            using (var context = m_Factory.Create())
            {
                context.CoinFiatValues.AddRange(values);
                context.SaveChanges();
            }
        }
    }
}
