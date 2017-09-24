using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;

namespace Msv.AutoMiner.Common.External
{
    public class AsyncRestClient : IAsyncRestClient
    {
        private const string JsonMime = "application/json";

        private readonly Uri m_BaseUrl;

        public AsyncRestClient(Uri baseUrl)
        {
            m_BaseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
        }

        public async Task<T> GetAsync<T>(string relativeUrl)
        {
            if (relativeUrl == null)
                throw new ArgumentNullException(nameof(relativeUrl));

            using (var client = CreateHttpClient())
            using (var response = await client.GetAsync(new Uri(m_BaseUrl, relativeUrl)))
            {
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
        }

        public async Task<TResponse> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest request)
        {
            if (relativeUrl == null)
                throw new ArgumentNullException(nameof(relativeUrl));
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            using (var client = CreateHttpClient())
            using (var response = await client.PostAsync(
                new Uri(m_BaseUrl, relativeUrl), new StringContent(JsonConvert.SerializeObject(request))
                {
                    Headers = {ContentType = new MediaTypeHeaderValue(JsonMime)}
                }))
            {
                response.EnsureSuccessStatusCode();
                return JsonConvert.DeserializeObject<TResponse>(await response.Content.ReadAsStringAsync());
            }
        }

        private static HttpClient CreateHttpClient()
        { 
            return new HttpClient
            {
                DefaultRequestHeaders =
                {
                    Accept = {new MediaTypeWithQualityHeaderValue(JsonMime, 1.0)}
                }      
            };
        }
    }
}
