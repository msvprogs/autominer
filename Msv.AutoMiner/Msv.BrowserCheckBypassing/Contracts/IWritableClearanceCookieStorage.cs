using System;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IWritableClearanceCookieStorage : IClearanceCookieStorage
    {
        void StoreCookie(Uri uri, ClearanceCookie cookie);
        ClearanceCookie GetCookieOrEmpty(Uri uri);
    }
}
