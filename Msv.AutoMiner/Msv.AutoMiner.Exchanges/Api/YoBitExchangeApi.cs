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
    //API: https://yobit.net/en/api/
    public class YoBitExchangeApi : WebExchangeApiBase
    {
        private static readonly DateTime M_StartDateForNonce = new DateTime(2016, 1, 1);

        private static readonly Uri M_BaseUri = new Uri("https://yobit.net/");
        private static readonly Uri M_PublicBaseUri = new Uri(M_BaseUri, "/api/3/");
        private static readonly Uri M_PrivateBaseUri = new Uri(M_BaseUri, "/tapi");

        public YoBitExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => JsonConvert.DeserializeObject<dynamic>(
                WebClient.DownloadString(new Uri(M_PublicBaseUri, method).ToString() + new QueryBuilder(parameters)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            parameters.Add("method", method);
            parameters.Add("nonce", ((long)(DateTime.UtcNow - M_StartDateForNonce).TotalSeconds).ToString());         
            var queryString = new QueryBuilder(parameters).ToStringWithoutPrefix();
            using (var hmac = new HMACSHA512(apiSecret))
            {
                var response = WebClient.UploadString(
                    M_PrivateBaseUri.ToString(),
                    queryString,
                    new Dictionary<string, string>
                    {
                        ["Key"] = apiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
                    },
                    contentType: "application/x-www-form-urlencoded");
                dynamic json = JsonConvert.DeserializeObject(response);
                if ((int)json.success != 1)
                    throw new ExternalDataUnavailableException((string)json.error);
                return json.@return;
            }
        }
    }
}
