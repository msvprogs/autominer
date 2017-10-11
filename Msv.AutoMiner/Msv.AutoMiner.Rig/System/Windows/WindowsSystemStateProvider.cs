using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Threading;
using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Data;

namespace Msv.AutoMiner.Rig.System.Windows
{
    public class WindowsSystemStateProvider : ISystemStateProvider
    {
        public CpuState[] GetCpuStates()
        {
            using (var processorSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_Processor"))
            using (var processorResults = processorSearcher.Get())
                return processorResults
                    .Cast<ManagementObject>()
                    .Select(x => new CpuState
                    {
                        Name = x["Name"].ToString(),
                        CurrentClockMhz = (int)(uint)x["CurrentClockSpeed"],
                        MaxClockMhz = (int)(uint)x["MaxClockSpeed"],
                        CoreUsages = Enumerable.Range(0, (int)(uint)x["NumberOfCores"])
                            .Select(y =>
                            {
                                using (var counter =
                                    new PerformanceCounter("Processor", "% Processor Time", y.ToString()))
                                {
                                    counter.NextValue();
                                    Thread.Sleep(50);
                                    return (int)counter.NextValue();
                                }
                            })
                            .ToArray()
                    })
                    .ToArray();
        }

        public double GetTotalMemoryMb()
        {
            using (var memorySearcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMemory"))
            using (var memoryResults = memorySearcher.Get())
                return memoryResults
                    .Cast<ManagementObject>()
                    .Select(x => (double)(ulong) x["Capacity"] / 1024 / 1024)
                    .DefaultIfEmpty(0)
                    .Sum();
        }

        public double GetUsedMemoryMb()
        {
            using (var counter = new PerformanceCounter("Memory", "Available MBytes"))
                return counter.NextValue();
        }
    }
}
