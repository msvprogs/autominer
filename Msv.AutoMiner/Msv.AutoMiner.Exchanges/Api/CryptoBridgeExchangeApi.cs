using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    public class CryptoBridgeExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_WebApiUri = new Uri("https://api.crypto-bridge.org/api/v1/");

        public CryptoBridgeExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters)
            => JsonConvert.DeserializeObject<dynamic>(WebClient.DownloadString(new Uri(M_WebApiUri, method)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
            => throw new NotSupportedException("Use WebSocket API to access private methods");
    }
}
