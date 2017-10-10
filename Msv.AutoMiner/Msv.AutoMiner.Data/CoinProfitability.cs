using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Msv.AutoMiner.Data
{
    public class CoinProfitability
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public int RigId { get; set; }

        public DateTime Requested { get; set; }

        public Guid CoinId { get; set; }

        public virtual Coin Coin { get; set; }

        public int PoolId { get; set; }

        public virtual Pool Pool { get; set; }

        public double CoinsPerDay { get; set; }

        public double BtcPerDay { get; set; }

        public double ElectricityCost { get; set; }

        public double UsdPerDay { get; set; }
    }
}
