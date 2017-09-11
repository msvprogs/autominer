using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Msv.AutoMiner.Service.System.Contracts;
using Msv.AutoMiner.Service.System.Windows.WinApi;

namespace Msv.AutoMiner.Service.System.Windows
{
    /// <summary>
    /// This class tracks all child processes and terminates them 
    /// even if parent process was terminated abnormally.
    /// </summary>
    public class WindowsChildProcessTracker : IChildProcessTracker
    {
        private readonly SafeKernelObjectHandle m_JobHandle;

        public WindowsChildProcessTracker()
        {
            m_JobHandle = Kernel32Native.CreateJobObject(IntPtr.Zero, null);
            if (m_JobHandle.IsInvalid)
                throw new Win32Exception("CreateJobObject failed");
            var info = new JobObjectExtendedLimitInformation
            {
                BasicLimitInformation = new JobObjectBasicLimitInformation
                {
                    LimitFlags = Kernel32Native.JobObjectLimitKillOnJobClose
                }
            };
            using (var infoHandle = SafeHGlobalHandle.FromStruct(info))
                if (!Kernel32Native.SetInformationJobObject(m_JobHandle, JobObjectInfoType.ExtendedLimitInformation,
                    infoHandle, (uint)Marshal.SizeOf<JobObjectExtendedLimitInformation>()))
                    throw new Win32Exception("SetInformationJobObject failed");
        }

        public void StartTracking(Process process)
        {
            if (process == null)
                throw new ArgumentNullException(nameof(process));

            if (!Kernel32Native.AssignProcessToJobObject(m_JobHandle, process.Handle))
                throw new Win32Exception("AssignProcessToJobObject failed");
        }

        public void Dispose() => m_JobHandle.Dispose();
    }
}
