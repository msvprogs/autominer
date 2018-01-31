using System;
using Msv.BrowserCheckBypassing.Contracts;
using Msv.HttpTools;

namespace Msv.BrowserCheckBypassing
{
    public class BrowserCheckBypasserFactory : IBrowserCheckBypasserFactory
    {
        private readonly IWritableClearanceCookieStorage m_ClearanceCookieStorage;

        public BrowserCheckBypasserFactory(IWritableClearanceCookieStorage clearanceCookieStorage)
        {
            m_ClearanceCookieStorage = clearanceCookieStorage ?? throw new ArgumentNullException(nameof(clearanceCookieStorage));
        }

        public IBrowserCheckBypasser Create(Uri uri, CorrectHttpException exception)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (exception == null) 
                throw new ArgumentNullException(nameof(exception));

            if (CloudflareBrowserCheckBypasser.IsCloudfareProtection(exception))
                return new CloudflareBrowserCheckBypasser(m_ClearanceCookieStorage);

            return null;
        }
    }
}
