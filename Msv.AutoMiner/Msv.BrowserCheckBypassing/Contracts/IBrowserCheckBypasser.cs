using System;
using System.Net;
using Msv.HttpTools;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IBrowserCheckBypasser
    {
        ClearanceCookie Solve(Uri uri, CookieContainer sourceCookies, CorrectHttpException responseException);
    }
}
