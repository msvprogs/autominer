using System.Collections.Generic;
using Msv.AutoMiner.Data;

namespace Msv.AutoMiner.ControlCenterService.External.Data
{
    public class MultiPoolInfo
    {
        public IReadOnlyDictionary<Pool, PoolInfo> PoolInfos { get; set; }
        public PoolCurrencyInfo[] CurrencyInfos { get; set; }
    }
}
