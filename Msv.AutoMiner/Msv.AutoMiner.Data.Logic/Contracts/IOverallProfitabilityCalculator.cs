using Msv.AutoMiner.Common.Enums;
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
