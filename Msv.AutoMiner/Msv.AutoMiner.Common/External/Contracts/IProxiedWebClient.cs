using System.Collections.Generic;

namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface IProxiedWebClient : IWebClient
    {
        string DownloadStringProxied(string url, Dictionary<string, string> headers = null);
    }
}