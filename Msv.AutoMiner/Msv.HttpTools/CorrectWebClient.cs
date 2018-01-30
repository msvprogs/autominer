using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class CorrectWebClient : WebClient, IBaseWebClient
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; rv:53.0) Gecko/20100101 Firefox/53.0";
        private const string AcceptEncodings = "gzip, deflate";

        private static readonly TimeSpan M_OrdinaryRequestTimeout = TimeSpan.FromSeconds(40);
        private static readonly TimeSpan M_MaxRequestTimeout =
#if DEBUG
            TimeSpan.FromMinutes(20);
#else
            TimeSpan.FromSeconds(70);
#endif
      
        public WebClient UnderlyingClient => this;
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        static CorrectWebClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 16;
            ServicePointManager.MaxServicePoints = 16;
            ServicePointManager.MaxServicePointIdleTime = 1000;
            ServicePointManager.SetTcpKeepAlive(false, 0, 0);
        }

        public CorrectWebClient()
            => Encoding = Encoding.UTF8;

        public Task<string> DownloadStringAsync(Uri uri, Dictionary<string, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            SetHeaders(headers);
            return DoTaskWithTimeoutAsync(DownloadStringTaskAsync(uri));
        }

        public Task<string> DownloadStringAsync(Uri uri, Dictionary<HttpRequestHeader, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            SetHeaders(headers);
            return DoTaskWithTimeoutAsync(DownloadStringTaskAsync(uri));
        }

        public Task<string> UploadStringAsync(Uri uri, string data, Dictionary<string, string> headers, NetworkCredential credentials = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            SetHeaders(headers);
            Credentials = credentials;
            return DoTaskWithTimeoutAsync(UploadStringTaskAsync(uri, data));
        }

        public Task<string> UploadStringAsync(Uri uri, string data, Dictionary<HttpRequestHeader, string> headers, NetworkCredential credentials = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            SetHeaders(headers);
            Credentials = credentials;
            return DoTaskWithTimeoutAsync(UploadStringTaskAsync(uri, data));
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            if (!(base.GetWebRequest(address) is HttpWebRequest request))
                return null;
            request.CookieContainer = CookieContainer;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Timeout = request.ReadWriteTimeout = (int)M_OrdinaryRequestTimeout.TotalMilliseconds;
            request.KeepAlive = false;
            return request;
        }

        private void SetHeaders(Dictionary<string, string> headers)
        {
            SetEssentialHeaders();
            foreach (var header in headers)
                Headers[header.Key] = header.Value;
        }

        private void SetHeaders(Dictionary<HttpRequestHeader, string> headers)
        {
            SetEssentialHeaders();
            foreach (var header in headers)
                Headers[header.Key] = header.Value;
        }

        private void SetEssentialHeaders()
        {
            Headers[HttpRequestHeader.UserAgent] = UserAgent;
            Headers[HttpRequestHeader.AcceptEncoding] = AcceptEncodings;
        }

        private async Task<string> DoTaskWithTimeoutAsync(Task<string> task)
        {
            var timeoutCancelSource = new CancellationTokenSource();
            var timeoutTask = Task.Delay(M_MaxRequestTimeout, timeoutCancelSource.Token);

            var resultTask = await Task.WhenAny(task, timeoutTask);
            if (resultTask == timeoutTask)
            {
                CancelAsync();
                throw new TimeoutException("WebClient operation timed out. All default timeouts were ignored (probably a .NET Core implementation bug)");
            }

            timeoutCancelSource.Cancel();
            return await task;
        }
    }
}
