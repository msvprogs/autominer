using System;
using Msv.AutoMiner.Common.Data.Enums;

namespace Msv.AutoMiner.Data.Logic
{
    public static class Extensions
    {
        public static Uri GetUrl(this Pool pool)
        {
            if (pool == null)
                throw new ArgumentNullException(nameof(pool));

            string scheme;
            switch (pool.Protocol)
            {
                case PoolProtocol.JsonRpc:
                    scheme = Uri.UriSchemeHttp;
                    break;
                case PoolProtocol.Stratum:
                    scheme = "stratum+tcp";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pool), "Unknown protocol");
            }
            var host = Uri.IsWellFormedUriString(pool.Host, UriKind.Absolute)
                ? new Uri(pool.Host, UriKind.Absolute).Host
                : pool.Host;
            return new UriBuilder { Scheme = scheme, Host = host, Port = pool.Port }.Uri;
        }

        public static string GetLogin(this Pool pool, Wallet miningTarget)
        {
            if (pool == null) 
                throw new ArgumentNullException(nameof(pool));

            if (!pool.IsAnonymous)
                return pool.WorkerLogin;
            if (miningTarget == null)
                return null;
            return string.IsNullOrEmpty(pool.WorkerLogin)
                ? miningTarget.Address
                : $"{miningTarget.Address}.{pool.WorkerLogin}";
        }
    }
}
