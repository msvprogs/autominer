using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Msv.AutoMiner.Common.External.Contracts
{
    public interface IWebClient
    {
        string DownloadString(
            string url, Encoding encoding = null, Dictionary<HttpRequestHeader, string> headers = null);

        string DownloadString(Uri url, Encoding encoding = null);

        string DownloadString(string url, Dictionary<string, string> headers);

        string UploadString(string url, string data, Dictionary<string, string> headers, NetworkCredential credentials = null);
    }
}
