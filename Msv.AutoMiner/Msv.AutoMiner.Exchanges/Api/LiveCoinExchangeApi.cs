using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://www.livecoin.net/api/public
    public class LiveCoinExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://api.livecoin.net/");

        public LiveCoinExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(
                WebClient.DownloadString(new Uri(M_BaseUri, method)));
            if (!(json is JObject) || json["success"] == null)
                return json;
            if (!(bool)json.success)
                throw new ExternalDataUnavailableException();
            return json;
        }

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            using (var hmac = new HMACSHA256(apiSecret))
            {
                // QueryBuilder doesn't escape comma, but this site requires it to be escaped
                var query = new QueryBuilder(parameters.OrderBy(x => x.Key)).ToString().Replace(",", "%2C");
                var response = WebClient.DownloadString(
                    new Uri(M_BaseUri, method + query).ToString(),
                    new Dictionary<string, string>
                    {
                        ["Api-Key"] = apiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(
                            Encoding.UTF8.GetBytes(query.Length > 0 ? query.Substring(1) : string.Empty))).ToUpperInvariant()
                    });
                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }
    }
}
