using System;

namespace Msv.AutoMiner.Rig.System.Video.NVidia.Nvml
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
