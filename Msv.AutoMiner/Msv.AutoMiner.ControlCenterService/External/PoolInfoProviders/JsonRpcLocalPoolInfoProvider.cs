using System;
using System.Linq;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Msv.AutoMiner.ControlCenterService.External.Contracts;
using Msv.AutoMiner.ControlCenterService.External.Data;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.ControlCenterService.External.PoolInfoProviders
{
    public class JsonRpcLocalPoolInfoProvider : IPoolInfoProvider
    {
        private const int LastTransactionCount = 40;

        private readonly IRpcClient m_RpcClient;

        public JsonRpcLocalPoolInfoProvider(IRpcClient rpcClient) 
            => m_RpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            var info = m_RpcClient.Execute<dynamic>("getinfo");
            var miningInfo = m_RpcClient.Execute<dynamic>("getmininginfo");
            var transactions = m_RpcClient.Execute<JArray>("listtransactions", string.Empty, LastTransactionCount)
                .Cast<dynamic>()
                .Where(x => (int) x.confirmations > 0)
                .Where(x => (string) x.category == "generate" || ((bool?) x.generated).GetValueOrDefault());

            return new PoolInfo
            {
                AccountInfo = new PoolAccountInfo
                {
                    ConfirmedBalance = ((double?) info.balance).GetValueOrDefault(),
                    UnconfirmedBalance = ((double?) info.newmint).GetValueOrDefault()
                        + ((double?)info.stake).GetValueOrDefault()
                },
                State = new PoolState
                {
                    LastBlock = (long?) info.blocks,
                    TotalWorkers = 1,
                    TotalHashRate = (long)(((double?)miningInfo.netmhashps).GetValueOrDefault() * 1e6)
                },
                PaymentsData = transactions
                    .Select(x => new PoolPaymentData
                    {
                        Amount = (double)x.amount,
                        DateTime = DateTimeHelper.ToDateTimeUtc((long)x.time),
                        Transaction = (string)x.transaction
                    })
                    .ToArray()
            };
        }
    }
}
