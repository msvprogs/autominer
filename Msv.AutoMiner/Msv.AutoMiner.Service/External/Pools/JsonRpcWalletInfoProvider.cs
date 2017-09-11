using System;
using Msv.AutoMiner.Commons.Data;
using Msv.AutoMiner.Service.Data;
using Msv.AutoMiner.Service.External.Contracts;
using Msv.AutoMiner.Service.External.Rpc;

namespace Msv.AutoMiner.Service.External.Pools
{
    public class JsonRpcWalletInfoProvider : IPoolInfoProvider
    {
        private readonly IRpcClient m_RpcClient;

        public JsonRpcWalletInfoProvider(Pool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            m_RpcClient = new JsonRpcClient(pool.Address, pool.Port, pool.WorkerLogin, pool.WorkerPassword);
        }

        public PoolInfo GetInfo(DateTime minPaymentDate)
        {
            var info = m_RpcClient.Execute<dynamic>("getinfo");
            return new PoolInfo(
                new PoolAccountInfo
                {
                    ConfirmedBalance = ((double?)info.balance).GetValueOrDefault(),
                    UnconfirmedBalance = ((double?)info.newmint).GetValueOrDefault()
                },
                new PoolState
                {
                    LastBlock = (long?)info.blocks,
                    TotalWorkers = 1
                },
                new PoolPaymentData[0]);
        }
    }
}
