using System;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IMiningWorkBuilderStorage
    {
        Wallet GetBitCoinMiningTarget();
        Pool[] GetActivePools(Guid[] coinIds);
    }
}
