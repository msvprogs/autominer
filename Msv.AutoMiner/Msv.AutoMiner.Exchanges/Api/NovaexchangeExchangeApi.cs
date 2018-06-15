using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: https://novaexchange.com/remote/faq/
    public class NovaexchangeExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://novaexchange.com/remote/v2/");

        public NovaexchangeExchangeApi(IWebClient webClient)
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => ProcessResponse(WebClient.DownloadString(new Uri(M_BaseUri, method)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            parameters.Add("nonce", CreateNonce());
            using (var hmac = new HMACSHA512(apiSecret))
            {
                var query = new QueryBuilder(parameters);
                var url = $"{M_BaseUri}private/{method}{query}";
                var postQuery = new QueryBuilder
                {
                    {"apikey", apiKey},
                    {"signature", Convert.ToBase64String(hmac.ComputeHash(Encoding.ASCII.GetBytes(url)))}
                };
                return ProcessResponse(WebClient.UploadString(
                    url, postQuery.ToStringWithoutPrefix(), null, contentType: "application/x-www-form-urlencoded"));
            }
        }

        private static dynamic ProcessResponse(string response)
        {
            var json = JsonConvert.DeserializeObject<dynamic>(response);
            if ((string)json.status != "success")
                throw new ExternalDataUnavailableException((string)json.message);
            return json;
        }
    }

}
