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
        public string GetOsName()
        {
            var osInfo = ExecuteWmiQuery("SELECT * FROM Win32_OperatingSystem")
                .FirstOrDefault();
            return osInfo != null
                ? $"{osInfo["Caption"]} (v{osInfo["Version"]})"
                : null;
        }

        public CpuState[] GetCpuStates()
            => ExecuteWmiQuery("SELECT * FROM Win32_Processor")
                .Select(x => new CpuState
                {
                    Name = x["Name"].ToString(),
                    CurrentClockMhz = (int) (uint) x["CurrentClockSpeed"],
                    MaxClockMhz = (int) (uint) x["MaxClockSpeed"],
                    CoreUsages = Enumerable.Range(0, (int) (uint) x["NumberOfCores"])
                        .Select(y =>
                        {
                            using (var counter =
                                new PerformanceCounter("Processor", "% Processor Time", y.ToString()))
                            {
                                counter.NextValue();
                                Thread.Sleep(50);
                                return (int) counter.NextValue();
                            }
                        })
                        .ToArray()
                })
                .ToArray();


        public double GetTotalMemoryMb()
            => ExecuteWmiQuery("SELECT * FROM Win32_PhysicalMemory")
                .Select(x => (double) (ulong) x["Capacity"] / 1024 / 1024)
                .DefaultIfEmpty(0)
                .Sum();

        public double GetUsedMemoryMb()
        {
            using (var counter = new PerformanceCounter("Memory", "Available MBytes"))
                return counter.NextValue();
        }

        private static ManagementObject[] ExecuteWmiQuery(string query)
        {
            using (var searcher = new ManagementObjectSearcher(query))
            using (var results = searcher.Get())
                return results
                    .Cast<ManagementObject>()
                    .ToArray();
        }
    }
}
