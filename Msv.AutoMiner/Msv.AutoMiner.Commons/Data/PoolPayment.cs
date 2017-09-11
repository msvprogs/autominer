using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Commons.Data
{
    [Table("poolpayments")]
    public class PoolPayment
    {
        [Key]
        [Column(Order = 0)]
        public int PoolId { get; set; }

        [Key]
        [Column(Order = 1)]
        public DateTime DateTime { get; set; }

        public double Amount { get; set; }

        public string Transaction { get; set; }

        public virtual Pool Pool { get; set; }
    }
}
