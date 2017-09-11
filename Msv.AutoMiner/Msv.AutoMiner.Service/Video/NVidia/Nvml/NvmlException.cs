using System;

namespace Msv.AutoMiner.Service.Video.NVidia.Nvml
{
    public class NvmlException : ApplicationException
    {
        public NvmlException(NvmlReturnValue returnValue)
            : base(returnValue.ToString())
        {
            if (returnValue == NvmlReturnValue.Success)
                throw new ArgumentOutOfRangeException(nameof(returnValue));
        }
    }
}
