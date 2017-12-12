using System;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IClearanceCookieStorage
    {
        ClearanceCookie GetCookie(Uri uri);
    }
}
