using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Infrastructure.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IMinerProcessController : IDisposable
    {
        CoinMiningInfo[] CurrentCoins { get; }
        MiningMode CurrentMode { get; }

        event EventHandler ProcessExited;
        void RunNew(Coin[] coins, Miner miner);
        void Stop();
    }
}
