using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://bittrex.com/Home/Api
    public class BittrexExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://bittrex.com");

        public BittrexExchangeApi(IWebClient webClient)
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => ProcessResponse(WebClient.DownloadString(new Uri(M_BaseUri, $"/api/v1.1/public/{method}")));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            parameters.Add("apikey", apiKey);
            parameters.Add("nonce", CreateNonce());

            var url = $"{M_BaseUri}api/v1.1/{method}{new QueryBuilder(parameters)}";
            using (var hmac = new HMACSHA512(apiSecret))
                return ProcessResponse(WebClient.DownloadString(
                    url,
                    new Dictionary<string, string>
                    {
                        ["apisign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(url)))
                    }));
        }

        private static dynamic ProcessResponse(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            if (!(bool)json.success)
                throw new ExternalDataUnavailableException((string)json.message);
            return json.result;
        }
    }
}
