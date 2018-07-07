using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Exchanges.Api
{
    //API: http://help.stocks.exchange/api-integration
    public class StocksExchangeExchangeApi : WebExchangeApiBase
    {
        private static readonly Uri M_BaseUri = new Uri("https://app.stocks.exchange/api2/");

        public StocksExchangeExchangeApi(IWebClient webClient) 
            : base(webClient)
        { }

        public override dynamic ExecutePublic(string method, IDictionary<string, string> parameters) 
            => JsonConvert.DeserializeObject<dynamic>(WebClient.DownloadString(new Uri(M_BaseUri, method)));

        public override dynamic ExecutePrivate(string method, IDictionary<string, string> parameters, string apiKey, byte[] apiSecret)
        {
            if (parameters == null) 
                throw new ArgumentNullException(nameof(parameters));

            parameters.Add("method", method);
            parameters.Add("nonce", CreateNonce());
            var serializedData = JsonConvert.SerializeObject(parameters);
            using (var hmac = new HMACSHA512(apiSecret))
            {
                var response = WebClient.UploadString(
                    M_BaseUri.ToString(),
                    serializedData,
                    new Dictionary<string, string>
                    {
                        ["Key"] = apiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(serializedData)))
                    },
                    contentType: "application/json");
                dynamic json = JsonConvert.DeserializeObject(response);
                if ((int)json.success == 0)
                    throw new ExternalDataUnavailableException((string)json.error ?? "Error");
                return json.data;
            }
        }
    }
}
