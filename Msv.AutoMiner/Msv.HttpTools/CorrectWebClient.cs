using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Msv.HttpTools.Contracts;

namespace Msv.HttpTools
{
    public class CorrectWebClient : IBaseWebClient
    {
        private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:58.0) Gecko/20100101 Firefox/58.0";
        private const string AcceptEncodings = "gzip, deflate";

        private static readonly TimeSpan M_OrdinaryRequestTimeout = TimeSpan.FromSeconds(40);
      
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
                using (var response = await client.GetAsync(uri))
                {
                    if (!response.IsSuccessStatusCode)
                        throw await CreateHttpException(response);
                    return await ReadContentAsString(response);
                }
            }
        }

        public async Task<string> UploadStringAsync(
            Uri uri, string data, Dictionary<string, string> headers, NetworkCredential credentials = null, string contentType = null)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            using (var client = CreateHttpClient(credentials))
            {
                SetHeaders(client, headers);
                using (var requestContent = new StringContent(data, Encoding, contentType))
                using (var response = await client.PostAsync(uri, requestContent))
                {
                    if (!response.IsSuccessStatusCode)
                        throw await CreateHttpException(response);
                    return await ReadContentAsString(response);
                }
            }
        }

        protected virtual HttpClientHandler CreateHttpClientHandler(NetworkCredential credentials)
            => new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                CookieContainer = CookieContainer,
                Credentials = credentials,
                ServerCertificateCustomValidationCallback = delegate { return true;}
            };

        private static async Task<CorrectHttpException> CreateHttpException(HttpResponseMessage message)
        {
            var body = new MemoryStream();
            if (message.Content != null)
            {
                await message.Content.CopyToAsync(body);
                body.Position = 0;
            }
            return new CorrectHttpException(
                message.StatusCode,
                message.ReasonPhrase,
                message.Headers
                    .ToDictionary(x => x.Key, x => string.Join(", ", x.Value)),
                body);
        }

        private HttpClient CreateHttpClient(NetworkCredential credentials = null)
            => new HttpClient(CreateHttpClientHandler(credentials))
            {
                Timeout = M_OrdinaryRequestTimeout
            };

        private static async Task<string> ReadContentAsString(HttpResponseMessage response)
        {
            // HttpClient doesn't recognize 'utf8' string as valid
            if ("utf8".Equals(response.Content.Headers.ContentType?.CharSet,
                StringComparison.InvariantCultureIgnoreCase))
                return Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync());

            return await response.Content.ReadAsStringAsync();
        }

        private static void SetHeaders(HttpClient client, Dictionary<string, string> headers)
        {
            SetEssentialHeaders(client);
            if (headers == null) 
                return;
            foreach (var header in headers)
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        private static void SetEssentialHeaders(HttpClient client)
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.AcceptEncoding.ParseAdd(AcceptEncodings);
        }
    }
}
