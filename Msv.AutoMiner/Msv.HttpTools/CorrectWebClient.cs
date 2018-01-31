using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class CorrectWebClient : IBaseWebClient
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
      
        public CookieContainer CookieContainer { get; set; } = new CookieContainer();

        public Encoding Encoding { get; set; }

        static CorrectWebClient()
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            ServicePointManager.Expect100Continue = false;
        }

        public CorrectWebClient()
            => Encoding = Encoding.UTF8;

        public async Task<string> DownloadStringAsync(Uri uri, Dictionary<string, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            using (var client = CreateHttpClient())
            {
                SetHeaders(client, headers);
                using (var response = (await client.GetAsync(uri)).EnsureSuccessStatusCode())
                    return await response.Content.ReadAsStringAsync();
            }
        }

        public async Task<string> UploadStringAsync(
            Uri uri, string data, Dictionary<string, string> headers, NetworkCredential credentials = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            using (var client = CreateHttpClient(credentials))
            {
                SetHeaders(client, headers);
                using (var requestContent = new StringContent(data, Encoding))
                using (var response = (await client.PostAsync(uri, requestContent)).EnsureSuccessStatusCode())
                    return await response.Content.ReadAsStringAsync();
            }
        }

        protected virtual HttpClientHandler CreateHttpClientHandler(NetworkCredential credentials)
            => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                CookieContainer = CookieContainer,
                Credentials = credentials
            };

        private HttpClient CreateHttpClient(NetworkCredential credentials = null)
            => new HttpClient(CreateHttpClientHandler(credentials))
            {
                Timeout = M_OrdinaryRequestTimeout
            };

        private static void SetHeaders(HttpClient client, Dictionary<string, string> headers)
        {
            SetEssentialHeaders(client);
            foreach (var header in headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        private static void SetEssentialHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd(AcceptEncodings);
        }

        //private async Task<string> DoTaskWithTimeoutAsync(Task<string> task)
        //{
        //    var timeoutCancelSource = new CancellationTokenSource();
        //    var timeoutTask = Task.Delay(M_MaxRequestTimeout, timeoutCancelSource.Token);

        //    var resultTask = await Task.WhenAny(task, timeoutTask);
        //    if (resultTask == timeoutTask)
        //    {
        //        CancelAsync();
        //        throw new TimeoutException("WebClient operation timed out. All default timeouts were ignored (probably a .NET Core implementation bug)");
        //    }

        //    timeoutCancelSource.Cancel();
        //    return await task;
        //}
    }
}
