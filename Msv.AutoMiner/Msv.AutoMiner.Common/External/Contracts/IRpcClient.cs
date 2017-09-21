namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface IRpcClient
    {
        TResponse Execute<TResponse>(string method, params object[] args);
    }
}
