using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //Public API: https://www.cryptopia.co.nz/Forum/Thread/255
    //Private API: https://www.cryptopia.co.nz/Forum/Thread/256
    public class CryptopiaExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://www.cryptopia.co.nz/api/");

        public CryptopiaExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => ProcessResponse(WebClient.DownloadString(new Uri(M_BaseUri, method)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            using (var hmac = new HMACSHA256(apiSecret))
            using (var md5 = MD5.Create())
            {
                const string requestJson = "{}";
                var url = M_BaseUri + method;
                var nonce = CreateNonce();
                var signature = Convert.ToBase64String(
                    hmac.ComputeHash(Encoding.UTF8.GetBytes(
                        string.Concat(apiKey,
                            "POST",
                            Uri.EscapeDataString(url).ToLowerInvariant(),
                            nonce,
                            Convert.ToBase64String(md5.ComputeHash(
                                Encoding.UTF8.GetBytes(requestJson)))))));
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
            if (!(bool)json.Success)
                throw new ExternalDataUnavailableException($"{(string)json.Message} / {(string)json.Error}");
            return json.Data;
        }
    }
}
