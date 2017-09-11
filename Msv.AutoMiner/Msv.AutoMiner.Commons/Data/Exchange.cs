using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("exchanges")]
    public class Exchange
    {
        [Key]
        public ExchangeType Type { get; set; }

        public string PublicKey { get; set; }

        public byte[] PrivateKey { get; set; }
    }
}