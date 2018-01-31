using System.Net;
using System.Net.Http;
using Msv.HttpTools;

namespace Msv.BrowserCheckBypassing
{
    internal class SolverWebClient : CorrectWebClient
    {
        protected override HttpClientHandler CreateHttpClientHandler(NetworkCredential credentials)
        {
            var handler = base.CreateHttpClientHandler(credentials);
            handler.AllowAutoRedirect = false;
            return handler;
        }
    }
}