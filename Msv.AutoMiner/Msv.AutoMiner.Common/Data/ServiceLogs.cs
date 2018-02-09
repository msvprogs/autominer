using System.Reflection;

namespace Msv.AutoMiner.Common.Data
{
    [Obfuscation(Exclude = true)]
    public class ServiceLogs
    {
        public string Full { get; set; }
        public string Errors { get; set; }
    }
}
