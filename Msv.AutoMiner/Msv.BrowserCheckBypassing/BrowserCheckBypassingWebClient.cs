using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Msv.BrowserCheckBypassing.Contracts;
using Msv.HttpTools.Contracts;

namespace Msv.BrowserCheckBypassing
{
    public class BrowserCheckBypassingWebClient : IBaseWebClient
    {
        public CookieContainer CookieContainer
        {
            get => m_BaseWebClient.CookieContainer;
            set => m_BaseWebClient.CookieContainer = value;
        }

        public Encoding Encoding
        {
            get => m_BaseWebClient.Encoding;
            set => m_BaseWebClient.Encoding = value;
        }

        private readonly IBaseWebClient m_BaseWebClient;
        private readonly IBrowserCheckBypasserFactory m_BypasserFactory;
        private readonly IWritableClearanceCookieStorage m_CookieStorage;

        public BrowserCheckBypassingWebClient(
            IBaseWebClient baseWebClient,
            IBrowserCheckBypasserFactory bypasserFactory,
            IWritableClearanceCookieStorage cookieStorage)
        {
            m_BaseWebClient = baseWebClient ?? throw new ArgumentNullException(nameof(baseWebClient));
            m_BypasserFactory = bypasserFactory ?? throw new ArgumentNullException(nameof(bypasserFactory));
            m_CookieStorage = cookieStorage ?? throw new ArgumentNullException(nameof(cookieStorage));
        }

        public async Task<string> DownloadStringAsync(Uri uri, Dictionary<string, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            try
            {
                LoadCookies(uri);
                return await m_BaseWebClient.DownloadStringAsync(uri, headers);
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                if (!TryToSolveChallenge(uri, wex.Response))
                    throw;
                return await DownloadStringAsync(uri, headers);
            }
        }

        public async Task<string> DownloadStringAsync(Uri uri, Dictionary<HttpRequestHeader, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            try
            {
                LoadCookies(uri);
                return await m_BaseWebClient.DownloadStringAsync(uri, headers);
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                if (!TryToSolveChallenge(uri, wex.Response))
                    throw;
                return await DownloadStringAsync(uri, headers);
            }
        }

        public async Task<string> UploadStringAsync(Uri uri, string data, Dictionary<string, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            try
            {
                LoadCookies(uri);
                return await m_BaseWebClient.UploadStringAsync(uri, data, headers);
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                if (!TryToSolveChallenge(uri, wex.Response))
                    throw;
                return await UploadStringAsync(uri, data, headers);
            }
        }

        public async Task<string> UploadStringAsync(Uri uri, string data, Dictionary<HttpRequestHeader, string> headers)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (headers == null)
                throw new ArgumentNullException(nameof(headers));

            try
            {
                LoadCookies(uri);
                return await m_BaseWebClient.UploadStringAsync(uri, data, headers);
            }
            catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                if (!TryToSolveChallenge(uri, wex.Response))
                    throw;
                return await UploadStringAsync(uri, data, headers);
            }
        }

        public void Dispose()
        { }

        private bool TryToSolveChallenge(Uri uri, WebResponse response)
        {
            var solver = m_BypasserFactory.Create(uri, (HttpWebResponse)response);
            if (solver == null)
                return false;
            try
            {
                solver.Solve(uri, CookieContainer, (HttpWebResponse) response);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void LoadCookies(Uri uri)
        {
            var clearanceCookie = m_CookieStorage.GetCookie(uri);
            if (clearanceCookie?.Id != null)
                CookieContainer.Add(uri, clearanceCookie.Id);
            if (clearanceCookie?.Clearance != null)
                CookieContainer.Add(uri, clearanceCookie.Clearance);
        }
    }
}
