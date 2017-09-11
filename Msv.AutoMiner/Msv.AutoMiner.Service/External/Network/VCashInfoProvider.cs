using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    public class VCashInfoProvider : IquidusInfoProvider
    {
        public VCashInfoProvider()
            : base("https://explorer.vchain.info/")
        { }

        protected override string GetTransactionUrl()
            => "/ext/getlasttxs/0.00000001";
    }
}
