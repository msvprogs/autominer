using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Msv.AutoMiner.Common.External
{
    // ReSharper disable once InconsistentNaming
    public class DDoSTriggerPreventingWebClient : LoggedWebClient
    {
        private static readonly Dictionary<HttpRequestHeader, string> M_Headers =
            new Dictionary<HttpRequestHeader, string>
            {
                [HttpRequestHeader.Accept] = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                [HttpRequestHeader.AcceptEncoding] = "gzip, deflate",
                [HttpRequestHeader.AcceptLanguage] = "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3"
            };

        private readonly bool m_AddDelay;
        private readonly ConcurrentDictionary<string, object> m_SyncRoots =
            new ConcurrentDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Random m_Random = new Random();

        public DDoSTriggerPreventingWebClient(bool addDelay)
        {
            m_AddDelay = addDelay;
        }

        public override string DownloadString(
            string url, Encoding encoding = null, TimeSpan? timeout = null, Dictionary<HttpRequestHeader, string> headers = null)
        {
            //Do not allow simultaneous requests on the same host, and make some pauses between them.
            lock (m_SyncRoots.GetOrAdd(new Uri(url).Host, x => new object()))
            {
                if (m_AddDelay)
                    Thread.Sleep(m_Random.Next(1000, 2000));
                return base.DownloadString(url, encoding, timeout, headers ?? M_Headers);
            }
        }
    }
}
