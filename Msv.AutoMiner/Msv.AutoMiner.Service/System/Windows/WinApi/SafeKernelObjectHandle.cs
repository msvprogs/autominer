using System;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;

namespace Msv.AutoMiner.Service.System.Windows.WinApi
{
    public class SafeKernelObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeKernelObjectHandle() 
            : base(true)
        { }

        public SafeKernelObjectHandle(IntPtr p)
            : this()
        {
            SetHandle(p);
        }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle() => Kernel32Native.CloseHandle(handle);
    }
}
