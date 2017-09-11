using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Msv.AutoMiner.Service.System.Windows.WinApi
{
    public class SafeHGlobalHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public static SafeHGlobalHandle FromAnsiString(string str)
        {
            var handle = new SafeHGlobalHandle();
            handle.SetHandle(Marshal.StringToHGlobalAnsi(str));
            return handle;
        }

        public static SafeHGlobalHandle FromSize(int size)
        {
            var handle = new SafeHGlobalHandle();
            handle.SetHandle(Marshal.AllocHGlobal(size));
            return handle;
        }

        public static SafeHGlobalHandle FromStruct<T>(T value)
            where T : struct
        {
            var handle = new SafeHGlobalHandle();
            handle.SetHandle(Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T))));
            Marshal.StructureToPtr(value, handle.DangerousGetHandle(), false);
            return handle;
        }

        private SafeHGlobalHandle() 
            : base(true)
        { }

        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
