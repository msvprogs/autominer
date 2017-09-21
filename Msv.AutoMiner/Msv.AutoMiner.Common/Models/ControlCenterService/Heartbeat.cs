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

        public VideoAdapterState[] VideoAdapterStates { get; set; }

        public class ValueWithReference<T>
        {
            public T Current { get; set; }

            public T Reference { get; set; }
        }

        public class ValueWithLimits<T>
        {
            public T Current { get; set; }

            public T Max { get; set; }
        }

        public class MiningState
        {
            public Guid CoinId { get; set; }

            public TimeSpan Duration { get; set; }

            public int ValidShares { get; set; }

            public int InvalidShares { get; set; }

            public ValueWithReference<long> HashRate { get; set; }
        }

        public class CpuState
        {
            public string Name { get; set; }

            public string SerialNumber { get; set; }

            public ValueWithReference<int> ClockMhz { get; set; }

            public int Utilization { get; set; }
        }

        public class VideoAdapterState
        {
            public string Name { get; set; }

            public string BiosVersion { get; set; }

            public ValueWithReference<int> CoreClockMhz { get; set; }

            public ValueWithReference<int> MemoryClockMhz { get; set; }

            public ValueWithLimits<double> PowerUsageWatts { get; set; }

            public ValueWithLimits<int> Temperature { get; set; }

            public ValueWithLimits<int> MemoryUsageMb { get; set; }

            public int FanSpeed { get; set; }

            public int Utilization { get; set; }

            public string PerformanceState { get; set; }
        }
    }
}
