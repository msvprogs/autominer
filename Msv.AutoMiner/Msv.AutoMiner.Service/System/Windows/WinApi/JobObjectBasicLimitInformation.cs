using System;
using System.Runtime.InteropServices;

namespace Msv.AutoMiner.Service.System.Windows.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    public struct JobObjectBasicLimitInformation
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public short LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public short ActiveProcessLimit;
        public long Affinity;
        public short PriorityClass;
        public short SchedulingClass;
    }
}
