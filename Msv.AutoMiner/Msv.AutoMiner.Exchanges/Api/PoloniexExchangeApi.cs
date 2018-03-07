using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.CustomExtensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://poloniex.com/support/api/
    public class PoloniexExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://poloniex.com");

        public PoloniexExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters)
            => JsonConvert.DeserializeObject<dynamic>(
                WebClient.DownloadString(new Uri(M_BaseUri, $"/public?command={method}")));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            parameters.Add("command", method);
            parameters.Add("nonce", CreateNonce());
            var queryString = new QueryBuilder(parameters).ToStringWithoutPrefix();
            using (var hmac = new HMACSHA512(apiSecret))
            {
                var response = WebClient.UploadString(
                    new Uri(M_BaseUri, "tradingApi").ToString(),
                    queryString,
                    new Dictionary<string, string>
                    {
                        ["Key"] = apiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
                    },
                    contentType: "application/x-www-form-urlencoded");
                dynamic json = JsonConvert.DeserializeObject(response);
                if (json.error != null)
                    throw new ExternalDataUnavailableException((string)json.error);
                return json;
            }
        }
    }
}
