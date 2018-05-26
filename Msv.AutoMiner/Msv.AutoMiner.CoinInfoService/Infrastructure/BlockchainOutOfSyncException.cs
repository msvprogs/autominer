using System;

namespace Msv.AutoMiner.CoinInfoService.Infrastructure
{
    public class BlockchainOutOfSyncException : ApplicationException
    {
        public DateTime LastBlockTime { get; }

        public BlockchainOutOfSyncException(DateTime lastBlockTime)
            : base(
                $"Provider blockchain is possibly out of sync: last block time is {lastBlockTime:R}, current time is {DateTime.UtcNow:R}") 
            => LastBlockTime = lastBlockTime;
    }
}
