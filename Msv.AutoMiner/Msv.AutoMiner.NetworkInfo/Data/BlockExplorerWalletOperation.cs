using System;

namespace Msv.AutoMiner.NetworkInfo.Data
{
    public class BlockExplorerWalletOperation
    {
        public string Id { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public string Address { get; set; }
        public string Transaction { get; set; }
    }
}
