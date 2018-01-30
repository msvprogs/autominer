using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.HttpTools.Contracts;
using NLog;

namespace Msv.AutoMiner.Common.External
{
    public class LoggedWebClient<T> : IWebClient
        where T : class, IBaseWebClient
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private readonly IBaseWebClientPool<T> m_Pool;

        public LoggedWebClient(IBaseWebClientPool<T> pool)
            => m_Pool = pool ?? throw new ArgumentNullException(nameof(pool));

        public virtual string DownloadString(
            string url, Encoding encoding = null, Dictionary<HttpRequestHeader, string> headers = null)
        {
            using (var webClient = m_Pool.Acquire())
            {
                if (encoding != null)
                    webClient.Value.Encoding = encoding;
                M_Logger.Debug($"GET {url}...");
                var response = webClient.Value.DownloadStringAsync(new Uri(url), headers ?? new Dictionary<HttpRequestHeader, string>())
                    .GetAwaiter().GetResult();
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        public string DownloadString(Uri url, Encoding encoding = null)
            => DownloadString(url.ToString(), encoding);

        public string DownloadString(string url, Dictionary<string, string> headers)
        {
            using (var webClient = m_Pool.Acquire())
            {
                M_Logger.Debug($"GET {url}...");
                var response = webClient.Value.DownloadStringAsync(new Uri(url), headers).GetAwaiter().GetResult();
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        public string UploadString(string url, string data, Dictionary<string, string> headers, NetworkCredential credentials = null)
        {
            using (var webClient = m_Pool.Acquire())
            {
                M_Logger.Debug($"POST {url}{Environment.NewLine}{data}");
                var response = webClient.Value.UploadStringAsync(new Uri(url), data, headers, credentials).GetAwaiter().GetResult();
                M_Logger.Debug($"POST {url} response ({webClient.Value.UnderlyingClient.ResponseHeaders[HttpResponseHeader.Server]}):{Environment.NewLine}{response}");
                return response;
            }
        }
     }
}