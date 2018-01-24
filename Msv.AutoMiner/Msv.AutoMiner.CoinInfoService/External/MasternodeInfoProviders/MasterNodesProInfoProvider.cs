using System;
using System.Globalization;
using System.Linq;
using Msv.AutoMiner.CoinInfoService.External.Contracts;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.Common.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.CoinInfoService.External.MasternodeInfoProviders
{
    public class MasternodesProInfoProvider : IMasternodeInfoProvider
    {
        private readonly IWebClient m_WebClient;

        public MasternodesProInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public MasternodeInfo[] GetMasternodeInfos()
        {
            dynamic stats = JsonConvert.DeserializeObject(
                m_WebClient.DownloadString("https://masternodes.pro/api/coins/stats?currency=USD&ver=2.0"));

            return ((JArray) stats.data)
                .Cast<dynamic>()
                .Select(x => new MasternodeInfo
                {
                    CurrencySymbol = (string) x.coin,
                    MasternodesCount = (int) ParsingHelper.ParseLong((string) x.total_mn),
                    TotalSupply = ParsingHelper.ParseDouble((string) x.coin_supply),
                    Updated = DateTime.Parse(((string) x.last_communication).Replace(" UTC", ""), CultureInfo.InvariantCulture)
                })
                .ToArray();
        }
    }
}
