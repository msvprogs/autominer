using System;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class Heartbeat
    {
        public DateTime DateTime { get; set; }

        public string OsVersion { get; set; }

        public string ClientVersion { get; set; }

        public MiningState[] MiningStates { get; set; }

        public CpuState[] CpuStates { get; set; }

        public ValueWithLimits<double> MemoryUsageMb { get; set; }

        public string VideoDriverVersion { get; set; }

        public VideoAdapterState[] VideoAdapterStates { get; set; }

        public struct ValueWithReference<T>
        {
            public T Current { get; set; }

            public T Reference { get; set; }
        }

        public struct ValueWithLimits<T>
        {
            public T Current { get; set; }

            public T Max { get; set; }
        }

        public class MiningState
        {
            public Guid CoinId { get; set; }

            public int PoolId { get; set; }

            public TimeSpan Duration { get; set; }

            public int ValidShares { get; set; }

            public int InvalidShares { get; set; }

            public ValueWithReference<long> HashRate { get; set; }
        }

        public class CpuState
        {
            public string Name { get; set; }

            public ValueWithReference<int> ClockMhz { get; set; }

            public int[] CoreUtilizations { get; set; }
        }

        public class VideoAdapterState
        {
            public string Name { get; set; }

            public string BiosVersion { get; set; }

            public ValueWithLimits<int> CoreClockMhz { get; set; }

            public ValueWithLimits<int> MemoryClockMhz { get; set; }

            public ValueWithLimits<double> PowerUsageWatts { get; set; }

            public ValueWithLimits<int> Temperature { get; set; }

            public ValueWithLimits<int> MemoryUsageMb { get; set; }

            public int FanSpeed { get; set; }

            public int Utilization { get; set; }

            public string PerformanceState { get; set; }
        }
    }
}
