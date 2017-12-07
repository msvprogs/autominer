using System.Threading.Tasks;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public interface ICoinInfoControllerStorage
    {
        Task<CoinAlgorithm[]> GetAlgorithms();
    }
}
