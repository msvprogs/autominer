using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    public class YoBitExchangeTrader : ExchangeTraderBase
    {
        private static readonly DateTime M_StartDateForNonce = new DateTime(2016, 1, 1);

        public YoBitExchangeTrader(string apiKey, string apiSecret)
            : base(apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override ExchangeAccountBalanceData[] GetBalances()
        {
            var response = DoRequest("getInfo", new Dictionary<string, string>());
            return ((JObject) response.funds).Properties()
                .Select(x => new {x.Name, Balance = x.Value.Value<double>()})
                .Join(((JObject) response.funds_incl_orders).Properties()
                    .Select(x => new {x.Name, BalanceWithOrders = x.Value.Value<double>()}),
                    x => x.Name, x => x.Name, (x, y) => new ExchangeAccountBalanceData
                    {
                        CurrencySymbol = x.Name,
                        Available = x.Balance,
                        OnOrders = y.BalanceWithOrders - x.Balance
                    })
                .ToArray();
        }

        //Not implemented in YoBit API?!
        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
            => new ExchangeAccountOperationData[0];

        private dynamic DoRequest(string command, IDictionary<string, string> parameters)
        {
            parameters.Add("method", command);
            parameters.Add("nonce", (DateTime.Now - M_StartDateForNonce).TotalSeconds.ToString("F0"));
            var queryString = string.Join("&", parameters.Select(x => $"{x.Key}={x.Value}"));
            using (var hmac = new HMACSHA512(ApiSecret))
            {
                var response = UploadString(
                    "https://yobit.net/tapi",
                    queryString,
                    new Dictionary<string, string>
                    {
                        ["Content-Type"] = "application/x-www-form-urlencoded",
                        ["Key"] = ApiKey,
                        ["Sign"] = HexHelper.ToHex(hmac.ComputeHash(Encoding.UTF8.GetBytes(queryString)))
                    });
                dynamic json = JsonConvert.DeserializeObject(response);
                if ((int)json.success != 1)
                    throw new ApplicationException((string)json.error);
                return json.@return;
            }
        }
    }
}
