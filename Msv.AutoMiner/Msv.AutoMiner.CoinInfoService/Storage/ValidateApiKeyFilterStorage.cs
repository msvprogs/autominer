using System.Linq;
using Msv.AutoMiner.Data;
using Msv.AutoMiner.Data.Logic;

namespace Msv.AutoMiner.CoinInfoService.Storage
{
    public class ValidateApiKeyFilterStorage : IValidateApiKeyFilterStorage
    {
        private readonly IAutoMinerDbContextFactory m_Factory;

        public ValidateApiKeyFilterStorage(IAutoMinerDbContextFactory factory)
            => m_Factory = factory;

        public ApiKey GetApiKey(string key)
        {
            using (var context = m_Factory.CreateReadOnly())
                return context.ApiKeys.FirstOrDefault(x => x.Key == key);
        }

        public void SaveApiKey(ApiKey key)
        {
            using (var context = m_Factory.Create())
            {
                context.Attach(key);
                context.SaveChanges();
            }
        }
    }
}
