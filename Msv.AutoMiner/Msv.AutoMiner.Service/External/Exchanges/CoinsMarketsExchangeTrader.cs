using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Service.External.Exchanges
{
    //API Secret Query String format: username=username&password=password&pin=pin
    public class CoinsMarketsExchangeTrader : ExchangeTraderBase
    {
        private readonly string m_ApiSecretQueryString;
        private readonly string[] m_Currencies;

        public CoinsMarketsExchangeTrader(string apiSecretQueryString, string[] currencies)
            : base("key", Encoding.ASCII.GetBytes(apiSecretQueryString))
        {
            if (currencies == null)
                throw new ArgumentNullException(nameof(currencies));

            m_ApiSecretQueryString = Uri.EscapeUriString(apiSecretQueryString);
            m_Currencies = currencies;
        }

        public override ExchangeAccountBalanceData[] GetBalances()
        {
            return ((JObject) DoPostRequest("gettradinginfo").funds)
                .Properties()
                .Select(x => new ExchangeAccountBalanceData
                {
                    CurrencySymbol = x.Name.ToUpperInvariant(),
                    Available = x.Value.Value<double>()
                })
                .ToArray();
        }

        public override ExchangeAccountOperationData[] GetOperations(DateTime startDate)
            => m_Currencies
                .SelectMany(x => ((JObject)DoPostRequest($"DepositLog[{x}]")).Properties())
                .Select(x => (dynamic)x.Value)
                .Select(x => new ExchangeAccountOperationData
                {
                    Amount = (double) x.amount,
                    CurrencySymbol = ((string) x.coin).ToUpperInvariant(),
                    DateTime = TimestampHelper.ToDateTime((long) x.timestamp)
                })
                .Concat(m_Currencies
                    .SelectMany(x => ((JObject)DoPostRequest($"WithdrawlLog[{x}]")).Properties())
                    .Select(x => (dynamic)x.Value)
                    .Select(x => new ExchangeAccountOperationData
                    {
                        Amount = -(double) x.amount,
                        CurrencySymbol = ((string) x.coin).ToUpperInvariant(),
                        DateTime = TimestampHelper.ToDateTime((long) x.timestamp)
                    }))
                .Where(x => x.DateTime > startDate)
                .ToArray();

        private dynamic DoPostRequest(string command)
        {
            dynamic response = JsonConvert.DeserializeObject(UploadString(
                "https://coinsmarkets.com/apiv1.php",
                $"{m_ApiSecretQueryString}&data={Uri.EscapeDataString(command)}",
                new Dictionary<string, string>
                {
                    ["Content-Type"] = "application/x-www-form-urlencoded"
                }));
            if ((int) response.success != 1)
                throw new ApplicationException((string) response.error ?? (string)response.@return);
            return response.@return;
        }
    }
}
