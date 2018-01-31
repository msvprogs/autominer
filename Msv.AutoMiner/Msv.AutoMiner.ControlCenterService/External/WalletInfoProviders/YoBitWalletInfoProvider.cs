using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class YoBitWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        private static readonly DateTime M_StartDateForNonce = new DateTime(2016, 1, 1);

        public YoBitWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret)
            : base(webClient, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
        {
            var response = DoRequest("getInfo", new Dictionary<string, string>());
            return ((JObject) response.funds).Properties()
                .Select(x => new {x.Name, Balance = x.Value.Value<double>()})
                .Join(((JObject) response.funds_incl_orders).Properties()
                    .Select(x => new {x.Name, BalanceWithOrders = x.Value.Value<double>()}),
                    x => x.Name, x => x.Name, (x, y) => new WalletBalanceData
                    {
                        CurrencySymbol = x.Name.ToUpperInvariant(),
                        Available = x.Balance,
                        Blocked = y.BalanceWithOrders - x.Balance
                    })
                .ToArray();
        }

        //Not implemented in YoBit API?!
        public override WalletOperationData[] GetOperations(DateTime startDate)
            => new WalletOperationData[0];

        private dynamic DoRequest(string command, IDictionary<string, string> parameters)
        {
            parameters.Add("method", command);
            parameters.Add("nonce", ((long)(DateTime.UtcNow - M_StartDateForNonce).TotalSeconds).ToString());
            var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"));
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = WebClient.UploadString(
                    "https://yobit.net/tapi",
                    queryString,
                    new Dictionary<string, string>
                    {
                        ["Key"] = ApiKey,
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
