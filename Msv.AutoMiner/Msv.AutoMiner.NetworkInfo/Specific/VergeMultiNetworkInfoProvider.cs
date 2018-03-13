using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.Data.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Common;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    [SpecificCoinInfoProvider("XVG")]
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
