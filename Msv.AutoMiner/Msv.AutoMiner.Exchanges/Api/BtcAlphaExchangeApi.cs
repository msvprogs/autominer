using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.CustomExtensions;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://btc-alpha.github.io/api-docs
    public class BtcAlphaExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://btc-alpha.com/api/");
        private static readonly TimeSpan M_RequestInterval = TimeSpan.FromSeconds(1);  // will it prevent 429?

        public BtcAlphaExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters)
        {
            parameters.Add("format", "json");
            Thread.Sleep(M_RequestInterval);
            return JsonConvert.DeserializeObject<dynamic>(
                WebClient.DownloadString($"{M_BaseUri}{method}{new QueryBuilder(parameters)}"));
        }

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null)
                throw new ArgumentNullException(nameof(parameters));

            using (var hmac = new HMACSHA256(apiSecret))
            {
                var query = new QueryBuilder(parameters.EmptyIfNull().OrderBy(x => x.Key));
                var response = WebClient.DownloadString(
                    new Uri(M_BaseUri, method + query).ToString(),
                    new Dictionary<string, string>
                    {
                        ["X-KEY"] = apiKey,
                        ["X-SIGN"] = HexHelper.ToHex(
                            hmac.ComputeHash(Encoding.UTF8.GetBytes(apiKey + query.ToStringWithoutPrefix()))),
                        ["X-NONCE"] = CreateNonce()
                    });
                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }
    }
}
