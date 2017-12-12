using System;
using System.Net;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IBrowserCheckBypasserFactory
    {
        IBrowserCheckBypasser Create(Uri uri, HttpWebResponse response);
    }
}