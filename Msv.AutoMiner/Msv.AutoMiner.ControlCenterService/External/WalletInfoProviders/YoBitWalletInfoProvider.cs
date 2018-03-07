using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Msv.AutoMiner.Exchanges.Api;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class YoBitWalletInfoProvider : ExchangeWalletInfoProviderBase
    {
        public YoBitWalletInfoProvider(IExchangeApi api, string apiKey, string apiSecret)
            : base(api, apiKey, Encoding.UTF8.GetBytes(apiSecret))
        { }

        public override WalletBalanceData[] GetBalances()
        {
            var response = DoRequest("getInfo");
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

        private dynamic DoRequest(string command, IDictionary<string, string> parameters = null) 
            => Api.ExecutePrivate(command, parameters ?? new Dictionary<string, string>(), ApiKey, ApiSecret);
    }
}
