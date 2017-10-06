﻿using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common
{
    public abstract class NetworkInfoProviderBase : INetworkInfoProvider
    {
        private const int BlocksCountForMeanTimeCalc = 5;

        protected BlockStats? CalculateBlockStats(IEnumerable<BlockInfo> blockInfos)
        {
            var lastBlocks = blockInfos
                .OrderByDescending(x => x.Timestamp)
                .Take(BlocksCountForMeanTimeCalc + 1)
                .ToArray();
            if (lastBlocks.Length <= 1)
                return null;

            var averageFromChain = lastBlocks
                .Zip(lastBlocks.Skip(1), (x, y) => x.Timestamp - y.Timestamp)
                .Average();
            // If current block is generating longer than the average of previous 5, consider it too
            var currentBlockTime = DateTimeHelper.NowTimestamp - lastBlocks.Max(x => x.Timestamp);
            var averageBlockTime = currentBlockTime > averageFromChain
                ? new[] {currentBlockTime, averageFromChain}.Average()
                : averageFromChain;

            var lastBlock = lastBlocks.OrderByDescending(x => x.Height).First();
            return new BlockStats
            {
                MeanBlockTime = averageBlockTime,
                Height = lastBlock.Height,
                LastReward = lastBlock.Reward
            };
        }

        public abstract CoinNetworkStatistics GetNetworkStats();
    }
}