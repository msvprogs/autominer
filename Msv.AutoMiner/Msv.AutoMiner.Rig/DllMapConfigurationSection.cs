using System.Configuration;

namespace Msv.AutoMiner.Rig
{
    //To allow .NET Framework to ignore dllmap configuration section used by Mono
    public class DllMapConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("os")]
        public string Os => (string) base["os"];

        [ConfigurationProperty("dll")]
        public string Dll => (string) base["dll"];

        [ConfigurationProperty("target")]
        public string Target => (string) base["target"];
    }
}
