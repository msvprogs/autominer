using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Data
{
    public class MultiCoinPoolCurrency
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int MultiCoinPoolId { get; set; }

        public virtual MultiCoinPool MultiCoinPool { get; set; }

        public bool IsIgnored { get; set; }

        [Required, MaxLength(64)]
        public string Symbol { get; set; }

        [Required, MaxLength(128)]
        public string Name { get; set; }

        [MaxLength(64)]
        public string Algorithm { get; set; }

        public int Port { get; set; }

        public int Workers { get; set; }

        public double Hashrate { get; set; }
    }
}
