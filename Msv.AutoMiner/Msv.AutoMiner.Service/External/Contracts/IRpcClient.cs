namespace Msv.AutoMiner.Service.External.Contracts
{
    public interface IRpcClient
    {
        TResponse Execute<TResponse>(string method, object[] args = null);
    }
}
