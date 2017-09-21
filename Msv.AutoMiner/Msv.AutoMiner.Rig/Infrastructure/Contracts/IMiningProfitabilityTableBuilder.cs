using Msv.AutoMiner.Rig.Data;

namespace Msv.AutoMiner.Rig.Infrastructure.Contracts
{
    public interface IMiningProfitabilityTableBuilder
    {
        CoinMiningData[] Build();
    }
}
