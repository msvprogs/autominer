using System;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.Common.External
{
    public class SessionedRpcClientFactory : ISessionedRpcClientFactory
    {
        public ISessionedRpcClient Create(Uri uri) 
            => new WebSocketJsonRpcClient(uri);
    }
}
