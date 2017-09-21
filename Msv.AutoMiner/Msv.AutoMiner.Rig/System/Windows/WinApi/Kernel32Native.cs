using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Rig.System.Windows.WinApi
{
    public static class Kernel32Native
    {
        public const int JobObjectLimitKillOnJobClose = 0x2000;

        private const string Kernel32 = "kernel32.dll";

        [DllImport(Kernel32, SetLastError = true)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        public static extern bool CloseHandle(IntPtr handle);

        [DllImport(Kernel32)]
        public static extern bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, int dwProcessGroupId);

        [DllImport(Kernel32, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern SafeKernelObjectHandle CreateJobObject(IntPtr securityAttributes, string lpName);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern bool SetInformationJobObject(
            SafeKernelObjectHandle hJob, JobObjectInfoType infoType,
            SafeHGlobalHandle lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport(Kernel32, SetLastError = true)]
        public static extern bool AssignProcessToJobObject(SafeKernelObjectHandle job, IntPtr process);
    }
}
