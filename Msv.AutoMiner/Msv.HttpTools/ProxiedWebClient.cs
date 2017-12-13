using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class ProxiedWebClient : CorrectWebClient, IProxiedBaseWebClient
    {
        private const int MaxAttempts = 8;

        private readonly IRoundRobin<ProxyInfo> m_ProxyInfos;

        public ProxiedWebClient(IRoundRobin<ProxyInfo> proxyInfos)
        {
            m_ProxyInfos = proxyInfos ?? throw new ArgumentNullException(nameof(proxyInfos));
        }

        public async Task<string> DownloadStringProxiedAsync(Uri uri, Dictionary<string, string> headers = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            for (var i = 0; i < MaxAttempts; i++)
            {
                var currentProxy = GetNextActiveProxy();
                try
                {
                    Proxy = new WebProxy(currentProxy.Uri);
                    var result = await DownloadStringAsync(uri, headers ?? new Dictionary<string, string>());
                    if (string.IsNullOrWhiteSpace(result))
                    {
                        //empty result = request limit detected
                        currentProxy.RecordFailure();
                        continue;
                    }
                    currentProxy.RecordSuccess();
                    return result;
                }
                catch (WebException)
                {
                    currentProxy.RecordFailure();
                }
            }
            throw new ProxyException("Couldn't download requested data through proxy, max attempts exceeded");
        }

        private ProxyInfo GetNextActiveProxy()
        {
            ProxyInfo proxy = null;
            for (var i = 0; i < m_ProxyInfos.Count; i++)
            {
                proxy = m_ProxyInfos.GetNext();
                if (proxy != null && proxy.CheckIsAlive())
                    break;
            }
            if (proxy == null || !proxy.CheckIsAlive())
                throw new ProxyException("No active proxy found");
            return proxy;
        }
    }
}
