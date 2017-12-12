using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IBaseWebClient : IDisposable
    {
        CookieContainer CookieContainer { get; set; }
        Encoding Encoding { get; set; }

        Task<string> DownloadStringAsync(Uri uri, Dictionary<string, string> headers);
        Task<string> DownloadStringAsync(Uri uri, Dictionary<HttpRequestHeader, string> headers);
        Task<string> UploadStringAsync(Uri uri, string data, Dictionary<string, string> headers);
        Task<string> UploadStringAsync(Uri uri, string data, Dictionary<HttpRequestHeader, string> headers);
    }
}
