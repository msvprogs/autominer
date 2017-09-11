using System;
using System.Collections.Generic;
using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface ICoinMarketInfoAggregatorStorage
    {
        Coin[] GetCoins();
        void StoreCurrentPrices(CoinBtcPrice[] prices);
        Dictionary<int, double> GetCoinMeanPrices(TimeSpan period);
    }
}
