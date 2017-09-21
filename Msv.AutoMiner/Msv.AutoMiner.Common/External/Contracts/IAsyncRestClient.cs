using System.Threading.Tasks;

namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface IAsyncRestClient
    {
        Task<T> GetAsync<T>(string relativeUrl);
        Task<TResponse> PostAsync<TRequest, TResponse>(string relativeUrl, TRequest request);
    }
}
