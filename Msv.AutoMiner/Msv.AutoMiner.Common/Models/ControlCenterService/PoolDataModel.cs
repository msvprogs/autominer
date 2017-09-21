using System;
using Msv.AutoMiner.Common.Enums;

namespace Msv.AutoMiner.Common.Models.ControlCenterService
{
    public class PoolDataModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public int Priority { get; set; }

        public double CoinsPerDay { get; set; }

        public double BtcPerDay { get; set; }

        public double ElectricityCost { get; set; }

        public double UsdPerDay { get; set; }

        public PoolProtocol Protocol { get; set; }
    }
}