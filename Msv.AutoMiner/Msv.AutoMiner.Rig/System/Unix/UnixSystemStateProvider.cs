using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Msv.AutoMiner.Rig.System.Contracts;
using Msv.AutoMiner.Rig.System.Data;
using NLog;

namespace Msv.AutoMiner.Rig.System.Unix
{
    public class UnixSystemStateProvider : ISystemStateProvider
    {
        private static readonly ILogger M_Log = LogManager.GetCurrentClassLogger();

        private const string CpuInfoPath = "/proc/cpuinfo";
        private const string CpuInfoFolder = "/sys/devices/system/cpu";
        private const string CpuCurrentFreqPath = "cpufreq/scaling_cur_freq";
        private const string CpuMaxFreqPath = "cpufreq/scaling_max_freq";
        private const string MemInfoPath = "/proc/meminfo";
        private const string DistribInfoPath = "/etc/lsb-release";

        public string GetOsName()
        {
            var osInfo = ParseKeyValuePairs(ReadFileLines(DistribInfoPath), "=")
                .ToLookup(x => x.Key, x => x.Value);
            var id = TryGetValue("distrib_id");
            var version = TryGetValue("distrib_release");
            var description = TryGetValue("distrib_description")?.Trim().Trim('"');
            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(version) && string.IsNullOrEmpty(description))
                return null;
            return $"{description} ({id} {version})";

            string TryGetValue(string key)
                => osInfo.Contains(key) ? osInfo[key].FirstOrDefault() : null;
        }

        public CpuState[] GetCpuStates()
        {
            return SplitBy(ReadFileLines(CpuInfoPath), string.Empty)
                .Select(x => ParseKeyValuePairs(x).ToDictionary(y => y.Key, y => y.Value))
                .Where(x => x.ContainsKey("physical id"))
                .GroupBy(x => x["physical id"])
                .Select(x =>
                {
                    var info = x.First();
                    return new CpuState
                    {
                        Name = info["model name"],
                        CurrentClockMhz = GetFrequencyValue(CpuCurrentFreqPath),
                        MaxClockMhz = GetFrequencyValue(CpuMaxFreqPath),
                        CoreUsages = Enumerable.Range(0, int.Parse(info["cpu cores"]))
                            .Select(y =>
                            {
                                using (var counter =
                                    new PerformanceCounter("Processor", "% Processor Time", y.ToString()))
                                {
                                    counter.NextValue();
                                    Thread.Sleep(100);
                                    return (int) counter.NextValue();
                                }
                            })
                            .ToArray()
                    };
                })
                .ToArray();

            int GetFrequencyValue(string infoPath)
            {
                var cpuInfoDirectory = new DirectoryInfo(CpuInfoFolder);
                return (int)cpuInfoDirectory.GetDirectories("cpu?")
                    .Concat(cpuInfoDirectory.GetDirectories("cpu??"))
                    .Select(y => ReadFileLines(Path.Combine(y.FullName, infoPath)).FirstOrDefault())
                    .Where(y => !string.IsNullOrEmpty(y))
                    .Select(y => long.Parse(y) / 1000.0)
                    .DefaultIfEmpty(0)
                    .Average();
            }
        }

        public double GetTotalMemoryMb()
            => GetMemoryValue("memtotal");

        public double GetUsedMemoryMb()
            => GetTotalMemoryMb() - GetMemoryValue("memavailable");

        private static double GetMemoryValue(string key)
        {
            var keyValuePair = ParseKeyValuePairs(ReadFileLines(MemInfoPath))
                .FirstOrDefault(x => x.Key == key);
            return keyValuePair.Value != null
                ? long.Parse(keyValuePair.Value.Split()[0]) / 1000.0
                : 0;
        }

        private static string[] ReadFileLines(string path)
        {
            if (!File.Exists(path))
            {
                M_Log.Warn($"File {path} doesn't exist");
                return new string[0];
            }
            try
            {
                return File.ReadAllLines(path);
            }
            catch (Exception ex)
            {
                M_Log.Error(ex);
                return new string[0];
            }
        }

        private static KeyValuePair<string, string>[] ParseKeyValuePairs(string[] lines, string separator = ":")
            => lines.Select(x => x.Split(separator.ToCharArray(), 2))
                .Where(x => x.Length == 2)
                .Select(x => new KeyValuePair<string, string>(x[0].Trim().ToLowerInvariant(), x[1].Trim()))
                .ToArray();

        private static IEnumerable<T[]> SplitBy<T>(IEnumerable<T> source, T splitValue)
        {
            var portion = new List<T>();
            foreach (var value in source)
            {
                if (!EqualityComparer<T>.Default.Equals(value, splitValue))
                {
                    portion.Add(value);
                    continue;
                }
                yield return portion.ToArray();
                portion.Clear();
            }
            yield return portion.ToArray();
        }
    }
}

