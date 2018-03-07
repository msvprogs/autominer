using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://www.southxchange.com/Home/Api
    public class SouthXchangeExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://www.southxchange.com/api/");

        public SouthXchangeExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => JsonConvert.DeserializeObject<dynamic>(WebClient.DownloadString(new Uri(M_BaseUri, method)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            throw new NotImplementedException();
        }
    }
}
