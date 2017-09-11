using Msv.AutoMiner.Service.External.Network.Common;

namespace Msv.AutoMiner.Service.External.Network
{
    public class MonaCoinInfoProvider : InsightInfoProvider
    {
        public MonaCoinInfoProvider() 
            : base("https://mona.chainsight.info/api")
        { }
    }
}
