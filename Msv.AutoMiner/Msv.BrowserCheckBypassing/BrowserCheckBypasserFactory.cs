using System;
using System.Net;
using Msv.BrowserCheckBypassing.Contracts;
using Msv.HttpTools.Contracts;

namespace Msv.BrowserCheckBypassing
{
    public class BrowserCheckBypasserFactory : IBrowserCheckBypasserFactory
    {
        private readonly IBaseWebClient m_SolveWebClient;
        private readonly IWritableClearanceCookieStorage m_ClearanceCookieStorage;

        public BrowserCheckBypasserFactory(IBaseWebClient solveWebClient, IWritableClearanceCookieStorage clearanceCookieStorage)
        {
            m_SolveWebClient = solveWebClient ?? throw new ArgumentNullException(nameof(solveWebClient));
            m_ClearanceCookieStorage = clearanceCookieStorage ?? throw new ArgumentNullException(nameof(clearanceCookieStorage));
        }

        public IBrowserCheckBypasser Create(Uri uri, HttpWebResponse response)
        {
            if (CloudflareBrowserCheckBypasser.IsCloudfareProtection(response))
                return new CloudflareBrowserCheckBypasser(m_SolveWebClient, m_ClearanceCookieStorage);

            return null;
        }
    }
}
