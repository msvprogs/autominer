using System;

namespace Msv.HttpTools
{
    public class ProxyException : ApplicationException
    {
        public ProxyException(string message)
            : base(message)
        { }
    }
}
