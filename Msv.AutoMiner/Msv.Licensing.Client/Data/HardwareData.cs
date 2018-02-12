namespace Msv.Licensing.Client.Data
{
    internal class HardwareData
    {
        public string ProcessorId { get; set; }
        public string ProcessorSignature { get; set; }
        public string MotherboardId { get; set; }
        public string MotherboardProductName { get; set; }
        public string[] MemoryIds { get; set; }
    }
}