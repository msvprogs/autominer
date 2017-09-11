using Msv.AutoMiner.Commons.Data;

namespace Msv.AutoMiner.Service.Storage.Contracts
{
    public interface IProfitabilityTableBuilderStorage
    {
        AlgorithmData[] GetAlgorithmDatas();
        AlgorithmPairData[] GetAlgorithmPairDatas();
    }
}
