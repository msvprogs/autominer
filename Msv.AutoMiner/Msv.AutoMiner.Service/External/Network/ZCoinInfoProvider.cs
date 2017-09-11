using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    public class ZCoinInfoProvider : IquidusInfoProvider
    {
        public ZCoinInfoProvider() 
            : base("http://explorer.zcoin.io")
        { }

        protected override string GetTransactionUrl() 
            => "/ext/getlasttxs/0.00000001";
    }
}
