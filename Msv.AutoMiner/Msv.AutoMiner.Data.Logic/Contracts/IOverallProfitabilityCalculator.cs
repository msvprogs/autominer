using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic.Contracts
{
    public interface IOverallProfitabilityCalculator
    {
        SingleProfitabilityData[] BuildProfitabilityTable(
            ValueAggregationType difficultyAggregation,
            ValueAggregationType priceAggregation);

        AlgorithmPowerDataCost[] CalculateTotalPower();
    }
}
