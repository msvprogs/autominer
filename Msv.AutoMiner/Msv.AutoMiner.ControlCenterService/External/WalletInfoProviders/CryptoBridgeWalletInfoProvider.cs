using System;
using System.Linq;
using Msv.AutoMiner.Common.External;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.WalletInfoProviders
{
    public class CryptoBridgeWalletInfoProvider : IWalletInfoProvider
    {
        private static readonly Uri M_WebSocketUri = new Uri("wss://eu.openledger.info/ws");

        private readonly ISessionedRpcClientFactory m_ClientFactory;
        private readonly string m_UserName;

        public CryptoBridgeWalletInfoProvider(ISessionedRpcClientFactory clientFactory, string userName)
        {
            m_ClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
            m_UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        }

        public WalletBalanceData[] GetBalances()
        {
            using (var client = m_ClientFactory.Create(M_WebSocketUri))
            {
                client.StartSession();

                if (!CallWsMethod<bool>(client, 1, "login", "", ""))
                    throw new ExternalDataUnavailableException("Login failed");
                CallWsMethod<int>(client, 1, "database");
                CallWsMethod<int>(client, 1, "network_broadcast");
                CallWsMethod<int>(client, 1, "history");

                var accountInfos = CallWsMethod<JArray>(client, 2, "get_full_accounts", new[] {m_UserName}, true);
                if (accountInfos.Count == 0)
                    return new WalletBalanceData[0];
                var balances = ((JArray)((JArray)accountInfos[0])[1]["balances"])
                    .ToDictionary(x => x["asset_type"].Value<string>(), x => x["balance"].Value<double>());
                if (balances.Count == 0)
                    return new WalletBalanceData[0];

                var assets = CallWsMethod<JArray>(client, 2, "get_objects", new object[] {balances.Keys.ToArray()})
                    .Cast<dynamic>()
                    .ToDictionary(x => (string) x.id, x => (currency: ((string) x.symbol).Split('.').Last(), precision: (int) x.precision));

                return balances
                    .Join(assets, x => x.Key, x => x.Key, (x, y) => new WalletBalanceData
                    {
                        CurrencySymbol = y.Value.currency.ToUpperInvariant(),
                        Available = x.Value / Math.Pow(10, y.Value.precision)
                    })
                    .ToArray();
            }
        }

        public WalletOperationData[] GetOperations(DateTime startDate)
            => new WalletOperationData[0];

        private static T CallWsMethod<T>(IRpcClient client, int code, string method, params object[] args)
            => client.Execute<T>("call", code, method, args);
    }
}
