using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Models.CoinInfoService;
using Msv.AutoMiner.Common.Models.ControlCenterService;
using Msv.AutoMiner.Data.Logic.Contracts;

namespace Msv.AutoMiner.Data.Logic
{
    public class MiningWorkBuilder : IMiningWorkBuilder
    {
        private static readonly TimeSpan M_MaxInactivityInterval = TimeSpan.FromHours(2);

        private readonly IMiningWorkBuilderStorage m_Storage;

        public MiningWorkBuilder(IMiningWorkBuilderStorage storage) 
            => m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));

        public MiningWorkModel[] Build(SingleProfitabilityData[] profitabilities, bool testMode)
        {
            if (profitabilities == null) 
                throw new ArgumentNullException(nameof(profitabilities));

            var coinIds = profitabilities
                .Where(x => testMode || DateTime.UtcNow - M_MaxInactivityInterval < x.LastUpdatedUtc)
                .Select(x => x.CoinId)
                .ToArray();
            var btcMiningTarget = m_Storage.GetBitCoinMiningTarget();
            return m_Storage.GetActivePools(coinIds)
                .GroupBy(x => x.Coin)
                .Join(profitabilities, x => x.Key.Id, x => x.CoinId,
                    (x, y) => (profitability:y, pools:x, miningTarget: x.Key.Wallets.FirstOrDefault(a => a.IsMiningTarget)))
                .Select(x => new MiningWorkModel
                {
                    CoinId = x.pools.Key.Id,
                    CoinName = x.pools.Key.Name,
                    CoinSymbol = x.pools.Key.Symbol,
                    CoinAlgorithmId = x.pools.Key.AlgorithmId,
                    Pools = x.pools.Select(y => CreatePoolDataModel(
                            y, x.profitability, y.UseBtcWallet ? btcMiningTarget : x.miningTarget))
                        .Where(y => y != null)
                        .ToArray()
                })
                .Where(x => x.Pools.Any())
                .ToArray();
        }

        private static PoolDataModel CreatePoolDataModel(Pool currentPool, SingleProfitabilityData profitability, Wallet miningTarget)
        {
            if (miningTarget == null)
                return null;

            var market = SelectAppropriateMarket(
                profitability.MarketPrices,
                currentPool.UseBtcWallet ? null : miningTarget.ExchangeType);
            return new PoolDataModel
            {
                Id = currentPool.Id,
                Name = currentPool.Name,
                Protocol = currentPool.Protocol,
                CoinsPerDay = CalculateValueWithPoolFee(profitability.CoinsPerDay, currentPool.FeeRatio),
                ElectricityCost = profitability.ElectricityCostPerDay,
                BtcPerDay = CalculateValueWithPoolFee(market?.BtcPerDay, currentPool.FeeRatio),
                UsdPerDay = CalculateValueWithPoolFee(market?.UsdPerDay, currentPool.FeeRatio),
                Priority = currentPool.Priority,
                Login = currentPool.GetLogin(miningTarget),
                Password = string.IsNullOrEmpty(currentPool.WorkerPassword) ? "x" : currentPool.WorkerPassword,
                Url = currentPool.GetUrl()
            };

            double CalculateValueWithPoolFee(double? value, double poolFee)
                => value.GetValueOrDefault() * (1 - poolFee / 100);

            MarketPriceData SelectAppropriateMarket(IEnumerable<MarketPriceData> markets, ExchangeType? exchangeType)
                => markets.Where(z => exchangeType == null || z.Exchange == exchangeType)
                    .OrderByDescending(z => z.BtcPerDay)
                    .FirstOrDefault();
        }
    }
}
