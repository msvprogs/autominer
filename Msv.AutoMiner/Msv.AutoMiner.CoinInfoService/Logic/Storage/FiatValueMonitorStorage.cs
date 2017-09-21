using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class FiatValueMonitorStorage : IFiatValueMonitorStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public FiatValueMonitorStorage(AutoMinerDbContext context)
            => m_Context = context;

        public Coin[] GetCoins()
        {
            return m_Context.Coins
                .Where(x => x.Activity != ActivityState.Deleted)
                .ToArray();
        }

        public FiatCurrency[] GetFiatCurrencies()
        {
            return m_Context.FiatCurrencies
                .ToArray();
        }

        public void StoreValues(CoinFiatValue[] values)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            m_Context.CoinFiatValues.AddRange(values);
            m_Context.SaveChanges();
        }
    }
}
