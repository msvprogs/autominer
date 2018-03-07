using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: http://coinexchangeio.github.io/slate
    public class CoinExchangeExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://www.coinexchange.io");

        public CoinExchangeExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters)
        {
            dynamic response = JsonConvert.DeserializeObject(
                WebClient.DownloadString(new Uri(M_BaseUri, $"/api/v1/{method}")));
            if ((int) response.success != 1)
                throw new ExternalDataUnavailableException((string) response.message);
            return response.result;
        }

        public override dynamic ExecutePrivate(
            string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret) 
            => throw new NotSupportedException("No private API at this time");
    }
}
