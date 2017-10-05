using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.FrontEnd.Models.Pools
{
    public class PoolDisplayModel : PoolBaseModel
    {
        public bool HasApi { get; set; }

        public ActivityState Activity { get; set; }

        public double ConfirmedBalance { get; set; }

        public double UnconfirmedBalance { get; set; }

        public long PoolHashRate { get; set; }

        public int PoolWorkers { get; set; }

        public int Priority { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}
