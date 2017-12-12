using System;
using System.Net;
using Msv.BrowserCheckBypassing.Contracts;

namespace Msv.BrowserCheckBypassing
{
    public class BrowserCheckBypasserFactory : IBrowserCheckBypasserFactory
    {
        private readonly IWritableClearanceCookieStorage m_ClearanceCookieStorage;

        public BrowserCheckBypasserFactory(IWritableClearanceCookieStorage clearanceCookieStorage)
        {
            m_ClearanceCookieStorage = clearanceCookieStorage ?? throw new ArgumentNullException(nameof(clearanceCookieStorage));
        }

        public IBrowserCheckBypasser Create(Uri uri, HttpWebResponse response)
        {
            if (CloudflareBrowserCheckBypasser.IsCloudfareProtection(response))
                return new CloudflareBrowserCheckBypasser(m_ClearanceCookieStorage);

            return null;
        }
    }
}
