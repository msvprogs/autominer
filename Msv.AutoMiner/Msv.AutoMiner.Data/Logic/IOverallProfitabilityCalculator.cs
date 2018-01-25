using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.Models.CoinInfoService;

namespace Msv.AutoMiner.Data.Logic
{
    public interface IOverallProfitabilityCalculator
    {
        SingleProfitabilityData[] BuildProfitabilityTable(
            ValueAggregationType difficultyAggregation,
            ValueAggregationType priceAggregation);

        AlgorithmPowerDataCost[] CalculateTotalPower();
    }
}
