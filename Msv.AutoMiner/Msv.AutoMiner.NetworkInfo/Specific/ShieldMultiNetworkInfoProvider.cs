using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.Enums;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.AutoMiner.NetworkInfo.Common;

namespace Msv.AutoMiner.NetworkInfo.Specific
{
    public class ShieldMultiNetworkInfoProvider : IquidusMultiAlgoNetworkInfoProvider
    {
        private static readonly Uri M_BaseUri = new Uri("http://188.226.178.216:3001/");

        private static readonly Dictionary<int, KnownCoinAlgorithm?> M_AlgoIds =
            new Dictionary<int, KnownCoinAlgorithm?>
            {
                [1] = KnownCoinAlgorithm.X17,
                [2] = KnownCoinAlgorithm.Lyra2Rev2,
                [3] = KnownCoinAlgorithm.Blake2S,
                [4] = KnownCoinAlgorithm.MyriadGroestl
            };

        public ShieldMultiNetworkInfoProvider(IWebClient webClient)
            : base(webClient, M_BaseUri, "XSH", M_AlgoIds)
        { }
    }
}
