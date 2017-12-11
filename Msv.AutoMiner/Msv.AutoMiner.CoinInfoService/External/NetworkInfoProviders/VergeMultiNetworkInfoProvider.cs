﻿using System;
using System.Collections.Generic;
using Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders.Common;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.CoinInfoService.External.NetworkInfoProviders
{
    public class VergeMultiNetworkInfoProvider : IquidusMultiAlgoNetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("https://verge-blockchain.info");

        private static readonly Dictionary<int, KnownCoinAlgorithm?> M_AlgoIds =
            new Dictionary<int, KnownCoinAlgorithm?>
            {
                [1] = KnownCoinAlgorithm.X17,
                [2] = KnownCoinAlgorithm.Lyra2Rev2,
                [3] = KnownCoinAlgorithm.Blake2S,
                [4] = KnownCoinAlgorithm.MyriadGroestl
            };

        public VergeMultiNetworkInfoProvider(IWebClient webClient)
            : base(webClient, M_BaseUri, "XVG", M_AlgoIds)
        { }
    }
}
