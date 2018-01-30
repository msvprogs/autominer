using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.HttpTools.Contracts;
using NLog;

namespace Msv.AutoMiner.Common.External
{
    public class ProxiedLoggedWebClient : LoggedWebClient<IProxiedBaseWebClient>, IProxiedWebClient
    {
        private readonly IBaseWebClientPool<IProxiedBaseWebClient> m_Pool;
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, string> M_Headers =
            new Dictionary<string, string>
            {
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                ["Accept-Language"] = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3"
            };

        public ProxiedLoggedWebClient(IBaseWebClientPool<IProxiedBaseWebClient> pool)
            : base(pool)
            => m_Pool = pool ?? throw new ArgumentNullException(nameof(pool));

        public string DownloadStringProxied(string url, Dictionary<string, string> headers = null)
        {
            using (var webClient = m_Pool.Acquire())
            {
                M_Logger.Debug($"Proxied GET {url}...");
                var response = webClient.Value.DownloadStringProxiedAsync(new Uri(url), headers ?? M_Headers)
                    .GetAwaiter().GetResult();
                M_Logger.Debug($"Proxied GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }
    }
}
