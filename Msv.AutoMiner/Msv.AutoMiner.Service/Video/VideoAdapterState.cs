namespace Msv.AutoMiner.Service.Video
{
    public class VideoAdapterState
    {
        public string Name { get; set; }
        public string VbiosVersion { get; set; }
        public uint GpuClocksMhz { get; set; }
        public uint GpuMaxClocksMhz { get; set; }
        public uint MemoryClocksMhz { get; set; }
        public uint MemoryMaxClocksMhz { get; set; }
        public uint Temperature { get; set; }
        public uint MaxTemperature { get; set; }
        public decimal PowerLimit { get; set; }
        public decimal PowerUsage { get; set; }
        public uint FanSpeed { get; set; }
        public uint TotalMemoryMb { get; set; }
        public uint UsedMemoryMb { get; set; }
        public uint GpuUtilization { get; set; }
        public uint? PerformanceState { get; set; }
    }
}
