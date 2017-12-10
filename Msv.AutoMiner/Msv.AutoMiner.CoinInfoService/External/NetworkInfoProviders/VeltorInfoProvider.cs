﻿using System;
using Msv.AutoMiner.CoinInfoService.External.Data;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class VeltorInfoProvider : NetworkInfoProviderBase
    {
        private readonly IWebClient m_WebClient;

        public VeltorInfoProvider(IWebClient webClient)
            => m_WebClient = webClient ?? throw new ArgumentNullException(nameof(webClient));

        public override CoinNetworkStatistics GetNetworkStats()
        {
            dynamic info = JsonConvert.DeserializeObject(m_WebClient.DownloadString(
                "https://veltor.suprnova.cc/index.php?page=api&action=getpoolstatus&api_key=48cb38c921c929a1b74babc0562a0996363d48f58d2e0f9f352a951eaaddf7ea"));
            return new CoinNetworkStatistics
            {
                Difficulty = (double) info.getpoolstatus.data.networkdiff,
                Height = (long) info.getpoolstatus.data.currentnetworkblock,
                NetHashRate = (double) info.getpoolstatus.data.nethashrate
            };
        }
    }
}
