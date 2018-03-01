using System;
using System.Collections.Generic;
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
        private const int IssueAssetOpcode = 14;
        private const int TransferAssetOpcode = 0;
        //TODO: add withdrawal opcode

        private const int BlockIntervalSeconds = 3;

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
                InitializeSession(client);
                var accountInfos = CallWsMethod<JArray>(client, 2, "get_full_accounts", new[] {m_UserName}, true);
                if (accountInfos.Count == 0)
                    return new WalletBalanceData[0];
                var balances = ((JArray)((JArray)accountInfos[0])[1]["balances"])
                    .ToDictionary(x => x["asset_type"].Value<string>(), x => x["balance"].Value<double>());
                if (balances.Count == 0)
                    return new WalletBalanceData[0];

                var assets = GetAssetsByIds(client, balances.Keys.ToArray());
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
        {
            using (var client = m_ClientFactory.Create(M_WebSocketUri))
            {
                InitializeSession(client);
                dynamic accountInfos = CallWsMethod<JArray>(client, 2, "get_full_accounts", new[] {m_UserName}, true);
                if (accountInfos.Count == 0)
                    return new WalletOperationData[0];

                string accountId = accountInfos[0][1].account.id;
                var accountOperations = CallWsMethod<JArray>(
                        client, 4, "get_account_history", accountId, "1.11.0", 100, "1.11.0")
                    .Cast<dynamic>()
                    .Where(x => (int) x.op[0] == IssueAssetOpcode
                                || (int) x.op[0] == TransferAssetOpcode)
                    .Where(x => (string) x.op[1].to == accountId
                                || (string) x.op[1].from == accountId
                                || (string) x.op[1].issue_to_account == accountId)
                    .Where(x => (int) x.result[0] == 0)
                    .Select(x => new
                    {
                        Data = x,
                        AmountData = x.op[1].amount ?? x.op[1].asset_to_issue,
                        IsWithdrawal = (string) x.op[1].from == accountId,
                    })
                    .Select(x => new
                    {
                        Id = (string) x.Data.id,
                        BlockNumber = (long)x.Data.block_num,
                        Amount = (long) x.AmountData.amount * (x.IsWithdrawal ? -1 : 1),
                        Asset = (string) x.AmountData.asset_id
                    })
                    .ToArray();
                if (accountOperations.Length == 0)
                    return new WalletOperationData[0];

                var assets = GetAssetsByIds(client, accountOperations.Select(x => x.Asset).Distinct().ToArray());
                return accountOperations
                    .Join(assets, x => x.Asset, x => x.Key, (x, y) => new WalletOperationData
                    {
                        CurrencySymbol = y.Value.currency.ToUpperInvariant(),
                        Amount = x.Amount / Math.Pow(10, y.Value.precision),
                        ExternalId = x.Id,
                        DateTime = BlockNumberToDate(x.BlockNumber)
                    })
                    .Where(x => x.DateTime > startDate)
                    .ToArray();
            }
        }

        private static void InitializeSession(ISessionedRpcClient client)
        {
            client.StartSession();

            if (!CallWsMethod<bool>(client, 1, "login", "", ""))
                throw new ExternalDataUnavailableException("Login failed");
            CallWsMethod<int>(client, 1, "database");
            CallWsMethod<int>(client, 1, "network_broadcast");
            CallWsMethod<int>(client, 1, "history");
        }

        private static Dictionary<string, (string currency, int precision)> GetAssetsByIds(IRpcClient client, string[] ids)
            => CallWsMethod<JArray>(client, 2, "get_objects", new object[] {ids})
                .Cast<dynamic>()
                .ToDictionary(
                    x => (string) x.id, 
                    x => (currency: ((string) x.symbol).Split('.').Last(), precision: (int) x.precision));

        private static T CallWsMethod<T>(IRpcClient client, int code, string method, params object[] args)
            => client.Execute<T>("call", code, method, args);

        private static DateTime BlockNumberToDate(long blockNumber)
        {
            //Assuming that block interval is exactly 3 seconds.
            //We know that 24841542 block was generated at Thu Mar 01 2018 08:52:33 GMT.
            var blockDifference = blockNumber - 24841542;
            return new DateTime(2018, 3, 1, 8, 52, 33, DateTimeKind.Utc).AddSeconds(blockDifference * BlockIntervalSeconds);
        }
    }
}
