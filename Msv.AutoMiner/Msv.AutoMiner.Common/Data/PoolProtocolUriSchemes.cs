using System;
using System.Collections.Generic;
using System.Linq;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Common.Data
{
    public static class PoolProtocolUriSchemes
    {
        private static readonly IDictionary<PoolProtocol, string> M_Schemes =
            new Dictionary<PoolProtocol, string>
            {
                [PoolProtocol.JsonRpc] = Uri.UriSchemeHttp,
                [PoolProtocol.Stratum] = "stratum+tcp",
                [PoolProtocol.XpmForAll] = "tcp"
            };

        public static string GetScheme(PoolProtocol protocol)
            => M_Schemes[protocol];

        public static bool HasScheme(string scheme)
            => M_Schemes.Any(x => x.Value.Equals(scheme, StringComparison.InvariantCultureIgnoreCase));
    }
}
