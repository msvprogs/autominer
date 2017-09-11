using System;
using System.Collections.Generic;
using System.Net;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using NLog;
using WebSocketSharp;

namespace Msv.AutoMiner.Service.External
{
    public abstract class WebDownloaderBase
    {
        private static readonly ILogger M_Logger = LogManager.GetLogger("WebDownloader");

        protected static string UserAgent => "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:53.0) Gecko/20100101 Firefox/53.0";

        static WebDownloaderBase()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }

        protected string ExecuteWebSocketRequest(WebSocket webSocket, string request)
        {
            if (webSocket.ReadyState != WebSocketState.Open)
                webSocket.Connect();
            var errorTask = Observable.FromEventPattern<ErrorEventArgs>(
                    x => webSocket.OnError += x, x => webSocket.OnError -= x)
                .Take(1)
                .ToTask();
            var responseTask = Observable.FromEventPattern<MessageEventArgs>(
                    x => webSocket.OnMessage += x, x => webSocket.OnMessage -= x)
                .Take(1)
                .Timeout(TimeSpan.FromSeconds(15))
                .ToTask();
            webSocket.Send(request);
            var result = Task.WhenAny(errorTask, responseTask).GetAwaiter().GetResult();
            if (result == responseTask)
                return responseTask.Result.EventArgs.Data;
            throw new WebException(errorTask.Result.EventArgs.Message + " " + errorTask.Result.EventArgs.Exception);
        }

        protected string DownloadString(string url, Encoding encoding = null, TimeSpan? timeout = null)
        {
            using (var webClient = new ExtendedWebClient(timeout))
            {
                if (encoding != null)
                    webClient.Encoding = encoding;
                M_Logger.Debug($"GET {url}...");
                var response = webClient.DownloadString(url);
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        protected string DownloadString(string url, Dictionary<HttpRequestHeader, string> headers)
        {
            using (var webClient = new ExtendedWebClient(null))
            {
                foreach (var header in headers)
                    webClient.Headers[header.Key] = header.Value;
                M_Logger.Debug($"GET {url}...");
                var response = webClient.DownloadString(url);
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        protected string DownloadString(string url, Dictionary<string, string> headers)
        {
            using (var webClient = new ExtendedWebClient(null))
            {
                foreach (var header in headers)
                    webClient.Headers[header.Key] = header.Value;
                M_Logger.Debug($"GET {url}...");
                var response = webClient.DownloadString(url);
                M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
                return response;
            }
        }

        protected string UploadString(string url, string data, Dictionary<string, string> headers, bool skipCertificateValidation = false)
        {
            using (var webClient = new ExtendedWebClient(null) {SkipCertificateValidation = skipCertificateValidation})
            {
                foreach (var header in headers)
                    webClient.Headers[header.Key] = header.Value;
                M_Logger.Debug($"POST {url}{Environment.NewLine}{data}");
                var response = webClient.UploadString(url, data);
                M_Logger.Debug($"POST {url} response:{Environment.NewLine}{response}");
                return response;
            }
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
                var request = base.GetWebRequest(address) as HttpWebRequest;
                if (request == null)
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
