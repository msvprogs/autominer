using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.CustomExtensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://graviex.net/documents/api_v2
    public class GraviexExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://graviex.net/api/v2/");

        public GraviexExchangeApi(IWebClient webClient)
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => ProcessResponse(WebClient.DownloadString(new Uri(M_BaseUri, method)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            parameters.Add("access_key", apiKey);
            parameters.Add("tonce", DateTimeHelper.NowTimestampMsec.ToString());

            var query = new QueryBuilder(parameters.OrderBy(x => x.Key));
            var targetUri = new Uri(M_BaseUri, method);
            using (var hmac = new HMACSHA256(apiSecret))
            {
                var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(
                    string.Join("|", "GET", targetUri.AbsolutePath, query.ToStringWithoutPrefix())));
                query.Add("signature", HexHelper.ToHex(signature));
                return ProcessResponse(WebClient.DownloadString(targetUri + query.ToString()));
            }
        }

        private static dynamic ProcessResponse(string response)
        {
            dynamic json = JsonConvert.DeserializeObject(response);
            if (json is JObject && json.error != null)
                throw new ExternalDataUnavailableException($"Error {(int)json.error.code}: {(string)json.error.message}");
            return json;
        }
    }
}
