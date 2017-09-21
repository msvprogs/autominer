using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public class ValidateApiKeyFilterStorage : IValidateApiKeyFilterStorage
    {
        private readonly AutoMinerDbContext m_Context;

        public ValidateApiKeyFilterStorage(AutoMinerDbContext context) 
            => m_Context = context;

        public Task<ApiKey> GetApiKey(string key) 
            => m_Context.ApiKeys.FirstOrDefaultAsync(x => x.Key == key);

        public async Task SaveApiKey(ApiKey key)
        {
            m_Context.Attach(key);
            await m_Context.SaveChangesAsync();
        }
    }
}
