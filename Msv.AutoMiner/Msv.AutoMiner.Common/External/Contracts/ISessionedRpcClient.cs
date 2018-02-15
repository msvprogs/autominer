using System;

namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface ISessionedRpcClient : IRpcClient, IDisposable
    {
        void StartSession();
    }
}
