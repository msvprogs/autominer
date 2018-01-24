using System;
using System.Collections.Concurrent;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.Logic.Storage.Contracts;
using Msv.AutoMiner.Common;

namespace Msv.AutoMiner.CoinInfoService.Logic.Storage
{
    public class MasternodeInfoMemoryStorage : IMasternodeInfoStorage
    {
        private static readonly TimeSpan M_InfoTtl = TimeSpan.FromHours(1.5);

        private readonly ConcurrentDictionary<string, MasternodeInfo> m_Infos =
            new ConcurrentDictionary<string, MasternodeInfo>(StringComparer.InvariantCultureIgnoreCase);

        public void Store(MasternodeInfo[] infos)
        {
            if (infos == null)
                throw new ArgumentNullException(nameof(infos));

            infos.Where(x => x.Updated < DateTime.UtcNow)
                .ForEach(x => m_Infos.AddOrUpdate(x.CurrencySymbol, x, (y, z) => z.Updated < x.Updated ? x : z));
        }

        public MasternodeInfo Load(string currencySymbol)
        {
            if (!m_Infos.TryGetValue(currencySymbol, out var info))
                return null;
            if (DateTime.UtcNow - info.Updated > M_InfoTtl)
                return null;
            return info;
        }
    }
}
