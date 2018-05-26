using System;

namespace Msv.AutoMiner.NetworkInfo
{
    public class NoPoWBlocksException : ApplicationException
    {
        public NoPoWBlocksException(string message) 
            : base(message)
        { }
    }
}
