﻿using Newtonsoft.Json;

namespace Msv.AutoMiner.NetworkInfo.Data
{
    public class BlockHeader
    {
        public string Hash { get; set; }
        public long Height { get; set; }
        public string Flags { get; set; }
        public string PreviousBlockHash { get; set; }
        public double Difficulty { get; set; }
        public long Time { get; set; }

        [JsonProperty("tx")]
        public string[] Transactions { get; set; }
    }
}
