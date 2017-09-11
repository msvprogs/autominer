using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Infrastructure.Contracts
{
    public interface IProfitabilityCalculator
    {
        double CalculateCoinsPerDay(Coin coin, long yourHashRate);
    }
}
