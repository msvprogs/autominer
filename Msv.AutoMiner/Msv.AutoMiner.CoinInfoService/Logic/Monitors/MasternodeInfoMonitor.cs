using System;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.Helpers;

namespace Msv.AutoMiner.CoinInfoService.Logic.Monitors
{
    public class MasternodeInfoMonitor : MonitorBase
    {
        private readonly IMasternodeInfoProviderFactory m_ProviderFactory;
        private readonly IMasternodeInfoStorage m_Storage;

        public MasternodeInfoMonitor(
            IMasternodeInfoProviderFactory providerFactory,
            IMasternodeInfoStorage storage) 
            : base(TimeSpan.FromMinutes(14))
        {
            m_ProviderFactory = providerFactory ?? throw new ArgumentNullException(nameof(providerFactory));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        protected override void DoWork()
        {
            var infos = EnumHelper.GetValues<MasternodeInfoSource>()
                .Select(x => (type:x, provider:m_ProviderFactory.Create(x)))
                .Select(x =>
                {
                    try
                    {
                        return ProcessSource(x.type, x.provider);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Couldn't get masternodes info from {x.type}");
                        return (x.type, infos:new MasternodeInfo[0]);
                    }
                })
                .SelectMany(x => x.infos)
                .GroupBy(x => x.CurrencySymbol, StringComparer.InvariantCultureIgnoreCase)
                .Select(x => x.OrderByDescending(y => y.Updated).First())
                .ToArray();

            m_Storage.Store(infos);
        }

        private (MasternodeInfoSource type, MasternodeInfo[] infos) ProcessSource(
            MasternodeInfoSource source, IMasternodeInfoProvider provider)
        {
            Log.Info($"Starting masternode info receiving from {source}...");
            var infos = provider.GetMasternodeInfos();
            Log.Info($"Got masternode infos for {infos.Length} currencies from {source}");
            return (source, infos);
        }
    }
}
