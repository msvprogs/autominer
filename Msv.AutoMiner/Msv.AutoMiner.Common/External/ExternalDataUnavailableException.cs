using System;

namespace Msv.AutoMiner.Common.External
{
    public class ExternalDataUnavailableException : ApplicationException
    {
        public ExternalDataUnavailableException()
        { }

        public ExternalDataUnavailableException(string message) 
            : base(message)
        { }
    }
}
