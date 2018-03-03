using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class GraviexWalletInfoProvider : ExchangeWalletInfoProviderBase
    {        
        private static readonly Uri M_BaseUri = new Uri("https://graviex.net/api/v2/");

        public GraviexWalletInfoProvider(IWebClient webClient, string apiKey, string apiSecret) 
            : base(webClient, apiKey, Encoding.ASCII.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
            => ((JArray) DoGetRequest("members/me.json").accounts)
                .Cast<dynamic>()
                .Select(x => new WalletBalanceData
                {
                    CurrencySymbol = ((string) x.currency).ToUpperInvariant(),
                    Available = (double) x.balance,
                    Blocked = (double) x.locked
                })
                .ToArray();

        //TODO: withdrawals
        public override WalletOperationData[] GetOperations(DateTime startDate) 
            => ((JArray) DoGetRequest("deposits.json"))
                .Cast<dynamic>()
                .Where(x => x.state == "accepted")
                .Select(x => new WalletOperationData
                {
                    DateTime = DateTimeOffset.Parse((string) x.created_at, CultureInfo.InvariantCulture).UtcDateTime,
                    ExternalId = (string) x.id,
                    CurrencySymbol = ((string) x.currency).ToUpperInvariant(),
                    Amount = (double) x.amount,
                    Transaction = (string) x.txid
                })
                .Where(x => x.DateTime > startDate)
                .ToArray();

        private dynamic DoGetRequest(string command, IDictionary<string, string> parameters = null)
        {
            if (parameters == null)
                parameters = new Dictionary<string, string>();

            parameters.Add("access_key", ApiKey);
            parameters.Add("tonce", DateTimeHelper.NowTimestampMsec.ToString());

            var query = new QueryBuilder(parameters.OrderBy(x => x.Key));
            var targetUri = new Uri(M_BaseUri, command);
            using (var hmac = new HMACSHA256(ApiSecret))
            {
                var signature = hmac.ComputeHash(Encoding.UTF8.GetBytes(
                    string.Join("|", "GET", targetUri.AbsolutePath, query.ToStringWithoutPrefix())));
                query.Add("signature", HexHelper.ToHex(signature));
                var response = WebClient.DownloadString(targetUri + query.ToString());
                dynamic json = JsonConvert.DeserializeObject(response);
                if (json is JObject && json.error != null)
                    throw new ExternalDataUnavailableException($"Error {(int)json.error.code}: {(string)json.error.message}");
                return json;
            }
        }
    }
}
