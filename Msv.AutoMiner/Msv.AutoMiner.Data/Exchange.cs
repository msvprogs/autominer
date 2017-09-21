using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Data
{
    public class Exchange
    {
        [Key]
        public ExchangeType Type { get; set; }

        public ActivityState Activity { get; set; }

        public string PublicKey { get; set; }

        public byte[] PrivateKey { get; set; }
    }
}