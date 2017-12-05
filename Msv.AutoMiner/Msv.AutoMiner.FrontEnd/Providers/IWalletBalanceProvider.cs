using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.FrontEnd.Providers
{
    public interface IWalletBalanceProvider
    {
        WalletBalance[] GetLastBalances();
        Dictionary<ExchangeType, DateTime> GetLastBalanceDates();
    }
}
