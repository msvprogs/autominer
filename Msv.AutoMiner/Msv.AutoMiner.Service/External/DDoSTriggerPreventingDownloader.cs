using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Msv.AutoMiner.Service.External.Contracts;

namespace Msv.AutoMiner.Service.External
{
    // ReSharper disable once InconsistentNaming
    public class DDoSTriggerPreventingDownloader : WebDownloaderBase, IDDoSTriggerPreventingDownloader
    {
        private static readonly Dictionary<HttpRequestHeader, string> M_Headers =
            new Dictionary<HttpRequestHeader, string>
            {
                [HttpRequestHeader.UserAgent] = UserAgent,
                [HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                [HttpRequestHeader.AcceptEncoding] = "gzip, deflate",
                [HttpRequestHeader.AcceptLanguage] = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3"
            };

        private readonly ConcurrentDictionary<string, object> m_SyncRoots =
            new ConcurrentDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Random m_Random = new Random();

        public string DownloadString(string url)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            //Do not allow simultaneous requests on the same host, and make some pauses between them.
            lock (m_SyncRoots.GetOrAdd(new Uri(url).Host, x => new object()))
            {
                Thread.Sleep(m_Random.Next(1000, 2000));
                return DownloadString(url, M_Headers);
            }
        }
    }
}
