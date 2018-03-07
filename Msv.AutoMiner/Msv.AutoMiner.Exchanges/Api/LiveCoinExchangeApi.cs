using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.CustomExtensions;
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
                var query = new QueryBuilder(parameters.EmptyIfNull().OrderBy(x => x.Key));
                var response = WebClient.DownloadString(
                    new Uri(M_BaseUri, method + query).ToString(),
                    new Dictionary<string, string>
                    {
                        ["Api-Key"] = apiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(
                            Encoding.UTF8.GetBytes(query.ToStringWithoutPrefix()))).ToUpperInvariant()
                    });
                return JsonConvert.DeserializeObject<dynamic>(response);
            }
        }
    }
}
