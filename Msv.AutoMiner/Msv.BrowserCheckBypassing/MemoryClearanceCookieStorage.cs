using System;
using System.Collections.Concurrent;
using Msv.BrowserCheckBypassing.Contracts;

namespace Msv.BrowserCheckBypassing
{
    public class MemoryClearanceCookieStorage : IWritableClearanceCookieStorage
    {
        public static IWritableClearanceCookieStorage Instance { get; } = new MemoryClearanceCookieStorage();

        private readonly ConcurrentDictionary<string, ClearanceCookie> m_Cookies =
            new ConcurrentDictionary<string, ClearanceCookie>(StringComparer.InvariantCultureIgnoreCase);

        private MemoryClearanceCookieStorage()
        { }

        public ClearanceCookie GetCookie(Uri uri) 
            => m_Cookies.TryGetValue(uri.Host, out var cookie) ? cookie : null;

        public void StoreCookie(Uri uri, ClearanceCookie cookie)
            => m_Cookies.AddOrUpdate(uri.Host, cookie, (x, y) => cookie);

        public ClearanceCookie GetCookieOrEmpty(Uri uri)
            => m_Cookies.GetOrAdd(uri.Host, new ClearanceCookie());
    }
}
