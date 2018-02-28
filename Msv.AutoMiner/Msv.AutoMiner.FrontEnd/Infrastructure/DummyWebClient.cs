using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Msv.AutoMiner.Common.External.Contracts;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class DummyWebClient : IProxiedWebClient
    {
        public string DownloadString(string url, Encoding encoding = null, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public string DownloadString(Uri url, Encoding encoding = null)
        {
            throw new NotImplementedException();
        }

        public string DownloadString(string url, Dictionary<string, string> headers)
        {
            throw new NotImplementedException();
        }

        public string UploadString(string url, string data, Dictionary<string, string> headers, NetworkCredential credentials = null,
            string contentType = null)
        {
            throw new NotImplementedException();
        }

        public string DownloadStringProxied(string url, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }

        public string DownloadStringProxied(Uri url, Dictionary<string, string> headers = null)
        {
            throw new NotImplementedException();
        }
    }
}
