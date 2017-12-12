﻿using System;
using System.Net;

namespace Msv.BrowserCheckBypassing
{
    internal class SolverWebClient : CorrectWebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            if (!(base.GetWebRequest(address) is HttpWebRequest request))
                return null;
            request.AllowAutoRedirect = false;
            return request;
        }
    }
}