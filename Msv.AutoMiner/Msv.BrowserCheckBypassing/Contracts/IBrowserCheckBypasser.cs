using System;
using System.Net;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IBrowserCheckBypasser
    {
        ClearanceCookie Solve(Uri uri, CookieContainer sourceCookies, HttpWebResponse challengeResponse);
    }
}
