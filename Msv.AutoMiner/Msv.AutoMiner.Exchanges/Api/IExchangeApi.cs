using System.Collections.Generic;

namespace Msv.AutoMiner.Exchanges.Api
{
    public interface IExchangeApi
    {
        dynamic ExecutePublic(string method, IDictionary<string, string> parameters);
        dynamic ExecutePrivate(
            string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret);
    }
}
