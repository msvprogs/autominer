using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Msv.HttpTools.Contracts
{
    public interface IProxiedBaseWebClient : IBaseWebClient
    {
        Task<string> DownloadStringProxiedAsync(Uri uri, Dictionary<string, string> headers = null);
    }
}
