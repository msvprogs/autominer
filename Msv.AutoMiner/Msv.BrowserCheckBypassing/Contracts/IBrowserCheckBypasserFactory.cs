using System;
using Msv.HttpTools;

namespace Msv.BrowserCheckBypassing.Contracts
{
    public interface IBrowserCheckBypasserFactory
    {
        IBrowserCheckBypasser Create(Uri uri, CorrectHttpException exception);
    }
}