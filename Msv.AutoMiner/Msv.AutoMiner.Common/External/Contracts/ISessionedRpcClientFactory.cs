using System;

namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface ISessionedRpcClientFactory
    {
        ISessionedRpcClient Create(Uri uri);
    }
}
