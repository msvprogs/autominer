using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Data
{
    public class BlockHeader
    {
        [JsonProperty("height")]
        public long Height { get; set; }

        [JsonProperty("flags")]
        public string Flags { get; set; }

        [JsonProperty("previousBlockHash")]
        public string PreviousBlockHash { get; set; }

        [JsonProperty("difficulty")]
        public double Difficulty { get; set; }

        [JsonProperty("time")]
        public long Time { get; set; }
    }
}
