namespace Msv.AutoMiner.Rig.Remote
{
    public interface IRestClient
    {
        T Get<T>(string relativeUrl);
        TResponse Post<TRequest, TResponse>(string relativeUrl, TRequest request);
    }
}
