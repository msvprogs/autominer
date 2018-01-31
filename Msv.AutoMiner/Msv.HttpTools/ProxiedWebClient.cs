using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class ProxiedWebClient : CorrectWebClient, IProxiedBaseWebClient
    {
        private const int MaxAttempts = 10;

        private readonly IRoundRobin<ProxyInfo> m_ProxyInfos;
        private WebProxy m_CurrentProxy;
        private ProxyInfo m_CurrentProxyInfo;

        public ProxiedWebClient(IRoundRobin<ProxyInfo> proxyInfos) 
            => m_ProxyInfos = proxyInfos ?? throw new ArgumentNullException(nameof(proxyInfos));

        public async Task<string> DownloadStringProxiedAsync(Uri uri, Dictionary<string, string> headers = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            for (var i = 0; i < MaxAttempts; i++)
            {
                m_CurrentProxyInfo = GetNextActiveProxy();
                m_CurrentProxy = new WebProxy(m_CurrentProxyInfo.Uri);
                try
                {
                    var result = await DownloadStringAsync(uri, headers ?? new Dictionary<string, string>());
                    if (string.IsNullOrWhiteSpace(result))
                        throw new ProxyException("Server returned empty result");
                    m_CurrentProxyInfo.RecordSuccess();
                    return result;
                }
                catch (HttpRequestException)
                {
                    m_CurrentProxyInfo.RecordFailure();
                }
                catch (CorrectHttpException)
                {
                    m_CurrentProxyInfo.RecordFailure();
                }
                catch (TaskCanceledException) // HttpClient throws this exception
                {
                    m_CurrentProxyInfo.RecordFailure();
                }
                catch (WebException)
                {
                    m_CurrentProxyInfo.RecordFailure();
                }
            }
            throw new ProxyException("Couldn't download requested data through proxy, max attempts exceeded");
        }

        protected override HttpClientHandler CreateHttpClientHandler(NetworkCredential credentials)
        {
            var handler = base.CreateHttpClientHandler(credentials);
            handler.Proxy = m_CurrentProxy ?? WebRequest.GetSystemWebProxy();
            return handler;
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
