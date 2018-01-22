using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public interface IValidateApiKeyFilterStorage
    {
        ApiKey GetApiKey(string key);
        void SaveApiKey(ApiKey key);
    }
}
