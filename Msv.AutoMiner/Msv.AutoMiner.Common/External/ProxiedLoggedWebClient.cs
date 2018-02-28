using System;
using System.Collections.Generic;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.HttpTools;
using Msv.HttpTools.Contracts;
using NLog;

namespace Msv.AutoMiner.Common.External
{
    public class ProxiedLoggedWebClient : LoggedWebClient, IProxiedWebClient
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<string, string> M_Headers =
            new Dictionary<string, string>
            {
                ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                ["Accept-Language"] = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3"
            };

        private readonly IRoundRobin<ProxyInfo> m_ProxyInfos;

        public ProxiedLoggedWebClient(IRoundRobin<ProxyInfo> proxyInfos) 
            => m_ProxyInfos = proxyInfos ?? throw new ArgumentNullException(nameof(proxyInfos));

        public string DownloadStringProxied(string url, Dictionary<string, string> headers = null)
            => DownloadStringProxied(new Uri(url), headers);

        public string DownloadStringProxied(Uri url, Dictionary<string, string> headers = null)
        {
            var webClient = new ProxiedWebClient(m_ProxyInfos);
            M_Logger.Debug($"Proxied GET {url}...");
            var response = webClient.DownloadStringProxiedAsync(url, headers ?? M_Headers)
                .GetAwaiter().GetResult();
            M_Logger.Debug($"Proxied GET {url} response:{Environment.NewLine}{response}");
            return response;  
        }
    }
}
