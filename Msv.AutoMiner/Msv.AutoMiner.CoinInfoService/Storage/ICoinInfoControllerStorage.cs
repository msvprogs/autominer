using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public interface ICoinInfoControllerStorage
    {
        CoinAlgorithm[] GetAlgorithms();
    }
}
