using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Msv.AutoMiner.Common.External.Contracts;
using NLog;

namespace Msv.AutoMiner.Common.External
{
    public class LoggedWebClient : IWebClient
    {
        private static readonly ILogger M_Logger = LogManager.GetCurrentClassLogger();

        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:53.0) Gecko/20100101 Firefox/53.0";
        private const string AcceptEncodings = "gzip, deflate";

        static LoggedWebClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        public virtual string DownloadString(
            string url, Encoding encoding = null, TimeSpan? timeout = null, Dictionary<HttpRequestHeader, string> headers = null)
        {
            using (var webClient = new ExtendedWebClient(timeout))
            {
                if (encoding != null)
                    webClient.Encoding = encoding;
                SetEssentialHeaders(webClient);
                if (headers != null)
                    foreach (var header in headers)
                        webClient.Headers[header.Key] = header.Value;
                M_Logger.Debug($"GET {url}...");
                var response = webClient.DownloadString(url);
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        public string DownloadString(Uri url, Encoding encoding = null, TimeSpan? timeout = null)
            => DownloadString(url.ToString(), encoding, timeout);

        public string DownloadString(string url, Dictionary<string, string> headers)
        {
            using (var webClient = new ExtendedWebClient(null))
            {
                SetEssentialHeaders(webClient);
                foreach (var header in headers)
                    webClient.Headers[header.Key] = header.Value;
                M_Logger.Debug($"GET {url}...");
                var response = webClient.DownloadString(url);
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        public string UploadString(string url, string data, Dictionary<string, string> headers, bool skipCertificateValidation = false)
        {
            using (var webClient = new ExtendedWebClient(null) {SkipCertificateValidation = skipCertificateValidation})
            {
                SetEssentialHeaders(webClient);
                foreach (var header in headers)
                    webClient.Headers[header.Key] = header.Value;
                M_Logger.Debug($"POST {url}{Environment.NewLine}{data}");
                var response = webClient.UploadString(url, data);
                M_Logger.Debug($"POST {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        private static void SetEssentialHeaders(WebClient webClient)
        {
            webClient.Headers[HttpRequestHeader.UserAgent] = UserAgent;
            webClient.Headers[HttpRequestHeader.AcceptEncoding] = AcceptEncodings;
        }

        private class ExtendedWebClient : WebClient
        {
            public bool SkipCertificateValidation { get; set; }

            private readonly TimeSpan m_Timeout;

            public ExtendedWebClient(TimeSpan? timeout)
            {
                m_Timeout = timeout.GetValueOrDefault(TimeSpan.FromSeconds(60));
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                if (!(base.GetWebRequest(address) is HttpWebRequest request))
                    return null;
                if (SkipCertificateValidation)
                    request.ServerCertificateValidationCallback = delegate { return true; };
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                request.Timeout = (int)m_Timeout.TotalMilliseconds;
                return request;
            }
        }
    }
}
