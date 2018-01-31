using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Msv.HttpTools.Contracts
{
    public interface IBaseWebClient
    {
        CookieContainer CookieContainer { get; set; }
        Encoding Encoding { get; set; }

        Task<string> DownloadStringAsync(Uri uri, Dictionary<string, string> headers);
        Task<string> UploadStringAsync(
            Uri uri, string data, Dictionary<string, string> headers, NetworkCredential credentials = null);
    }
}
