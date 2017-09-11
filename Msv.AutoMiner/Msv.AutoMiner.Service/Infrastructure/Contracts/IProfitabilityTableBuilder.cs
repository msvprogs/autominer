using Msv.AutoMiner.Service.Infrastructure.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IProfitabilityTableBuilder
    {
        CoinProfitabilityData[] Build();
    }
}
