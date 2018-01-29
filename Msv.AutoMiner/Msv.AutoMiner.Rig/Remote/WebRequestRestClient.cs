using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Rig.Remote
{
    public class WebRequestRestClient : IRestClient
    {
        private const string JsonMime = "application/json";

        public X509Certificate2 ClientCertificate { get; set; }

        private readonly Uri m_BaseUrl;

        static WebRequestRestClient() 
            => ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

        public WebRequestRestClient(Uri baseUrl) 
            => m_BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));

        public T Get<T>(string relativeUrl)
        {
            if (relativeUrl == null)
                throw new ArgumentNullException(nameof(relativeUrl));

            using (var client = new ExtendedWebClient(ClientCertificate))
            {
                return JsonConvert.DeserializeObject<T>(
                    client.DownloadString(new Uri(m_BaseUrl, relativeUrl)));
            }
        }

        public Stream GetStream(string relativeUrl)
        {
            if (relativeUrl == null)
                throw new ArgumentNullException(nameof(relativeUrl));

            //HttpClient doesn't work correctly in Mono
            var tempFile = Path.GetTempFileName();
            using (var client = new ExtendedWebClient(ClientCertificate))
            {
                client.DownloadFile(new Uri(m_BaseUrl, relativeUrl), tempFile);
                return new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.None, 4096, FileOptions.DeleteOnClose);
            }
        }

        public TResponse Post<TRequest, TResponse>(string relativeUrl, TRequest request)
        {
            if (relativeUrl == null)
                throw new ArgumentNullException(nameof(relativeUrl));

            using (var client = new ExtendedWebClient(ClientCertificate))
            {
                client.Headers[HttpRequestHeader.ContentType] = JsonMime;
                return JsonConvert.DeserializeObject<TResponse>(
                    client.UploadString(new Uri(m_BaseUrl, relativeUrl), JsonConvert.SerializeObject(request)));
            }
        }

        private class ExtendedWebClient : WebClient
        {
            private readonly X509Certificate2 m_ClientCertificate;

            public ExtendedWebClient(X509Certificate2 clientCertificate)
            {
                m_ClientCertificate = clientCertificate;
                Encoding = Encoding.UTF8;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var request = (HttpWebRequest) base.GetWebRequest(address);
                if (request == null)
                    return null;
                if (m_ClientCertificate != null)
                    request.ClientCertificates.Add(m_ClientCertificate);
                return request;
            }
        }
    }
}
