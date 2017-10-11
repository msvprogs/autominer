namespace Msv.AutoMiner.Rig.System.Data
{
    public class CpuState
    {
        public string Name { get; set; }
        public int[] CoreUsages { get; set; }
        public int Temperature { get; set; }
        public int CurrentClockMhz { get; set; }
        public int MaxClockMhz { get; set; }
    }
}
