using System.Threading.Tasks;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public interface IValidateApiKeyFilterStorage
    {
        Task<ApiKey> GetApiKey(string key);
        Task SaveApiKey(ApiKey key);
    }
}
