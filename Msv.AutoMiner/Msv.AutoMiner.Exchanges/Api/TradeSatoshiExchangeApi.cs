using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://tradesatoshi.com/Home/Api
    public class TradeSatoshiExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://tradesatoshi.com/api/");

        public TradeSatoshiExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters)
            => ProcessResponse(WebClient.DownloadString(new Uri(M_BaseUri, $"public/{method}")));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            using (var hmac = new HMACSHA512(apiSecret))
            {
                var requestJson = JsonConvert.SerializeObject(parameters);
                var url = new Uri(M_BaseUri, $"private/{method}").ToString();
                var nonce = CreateNonce();
                var signature = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(
                        string.Concat(apiKey,
                            "POST",
                            Uri.EscapeDataString(url).ToLowerInvariant(),
                            nonce,
                            Convert.ToBase64String(Encoding.UTF8.GetBytes(requestJson))))));
                return ProcessResponse(WebClient.UploadString(
                    url,
                    requestJson,
                    new Dictionary<string, string>
                    {
                        ["Authorization"] = $"amx {apiKey}:{signature}:{nonce}"
                    },
                    contentType: "application/json"));
            }
        }

        private static dynamic ProcessResponse(string response)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(response);
            if (!(bool)json.success)
                throw new ExternalDataUnavailableException((string)json.message);
            return json.result;
        }
    }
}
