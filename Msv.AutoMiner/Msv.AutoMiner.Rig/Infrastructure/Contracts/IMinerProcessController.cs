using System;
using Msv.AutoMiner.Rig.Data;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMinerProcessController : IDisposable
    {
        MiningState CurrentState { get; }

        event EventHandler ProcessExited;
        void RunNew(CoinMiningData miningData);
        void Stop();
    }
}
