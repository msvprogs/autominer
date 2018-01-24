using System;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.MasternodeInfoProviders;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.CoinInfoService.External
{
    public class MasternodeInfoProviderFactory : IMasternodeInfoProviderFactory
    {
        private readonly IWebClient m_WebClient;

        public MasternodeInfoProviderFactory(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public IMasternodeInfoProvider Create(MasternodeInfoSource source)
        {
            switch (source)
            {
                case MasternodeInfoSource.MasternodesPro:
                    return new MasternodesProInfoProvider(m_WebClient);
                default:
                    return new DummyMasternodeInfoProvider();
            }
        }

        private class DummyMasternodeInfoProvider : IMasternodeInfoProvider
        {
            public MasternodeInfo[] GetMasternodeInfos()
                => new MasternodeInfo[0];
        }
    }
}
