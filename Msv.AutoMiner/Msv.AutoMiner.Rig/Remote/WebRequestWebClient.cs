using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Msv.AutoMiner.Common;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.Rig.Remote
{
    public class WebRequestWebClient : IWebClient
    {
        public string DownloadString(string url, Encoding encoding = null, Dictionary<string, string> headers = null)
        {
            using (var webClient = new WebClient())
            {
                if (encoding != null)
                    webClient.Encoding = encoding;
                headers?.ForEach(x => webClient.Headers[x.Key] = x.Value);
                return webClient.DownloadString(url);
            }
        }

        public string DownloadString(Uri url, Encoding encoding = null)
            => DownloadString(url.ToString(), encoding);

        public string DownloadString(string url, Dictionary<string, string> headers)
            => DownloadString(url, null, headers);

        public string UploadString(string url, string data, Dictionary<string, string> headers, NetworkCredential credentials = null, string contentType = null)
        {
            using (var webClient = new WebClient())
            {
                if (contentType != null)
                    webClient.Headers[HttpRequestHeader.ContentType] = contentType;
                headers?.ForEach(x => webClient.Headers[x.Key] = x.Value);
                webClient.Credentials = credentials;
                return webClient.UploadString(url, data);
            }
        }
    }
}
