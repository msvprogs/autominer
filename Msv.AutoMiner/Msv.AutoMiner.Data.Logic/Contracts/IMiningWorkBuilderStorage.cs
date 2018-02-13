using System;

namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IMiningWorkBuilderStorage
    {
        Wallet GetBitCoinMiningTarget();
        Pool[] GetActivePools(Guid[] coinIds);
    }
}
