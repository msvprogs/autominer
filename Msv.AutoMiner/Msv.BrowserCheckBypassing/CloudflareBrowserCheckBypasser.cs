using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using HtmlAgilityPack;
using Jint;
using Msv.BrowserCheckBypassing.Contracts;

namespace Msv.BrowserCheckBypassing
{
    internal class CloudflareBrowserCheckBypasser : IBrowserCheckBypasser
    {
        private const string IdCookieName = "__cfduid";
        private const string ClearanceCookieName = "cf_clearance";

        private static readonly string M_InitJs;

        private readonly IWritableClearanceCookieStorage m_ClearanceCookieStorage;

        static CloudflareBrowserCheckBypasser()
        {
            using (var jsStream = typeof(CloudflareBrowserCheckBypasser).Assembly
                .GetManifestResourceStream(typeof(CloudflareBrowserCheckBypasser), "cloudflare.js"))
            using (var reader = new StreamReader(jsStream))
                M_InitJs = reader.ReadToEnd();
        }

        public static bool IsCloudfareProtection(HttpWebResponse response)
            => response.StatusCode == HttpStatusCode.ServiceUnavailable
               && !string.IsNullOrWhiteSpace(response.Headers["CF-RAY"]);

        public CloudflareBrowserCheckBypasser(IWritableClearanceCookieStorage clearanceCookieStorage)
            => m_ClearanceCookieStorage = clearanceCookieStorage ?? throw new ArgumentNullException(nameof(clearanceCookieStorage));

        public ClearanceCookie Solve(Uri uri, CookieContainer sourceCookies, HttpWebResponse challengeResponse)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));
            if (sourceCookies == null)
                throw new ArgumentNullException(nameof(sourceCookies));
            if (challengeResponse == null)
                throw new ArgumentNullException(nameof(challengeResponse));

            var cookie = m_ClearanceCookieStorage.GetCookieOrEmpty(uri);
            lock (cookie)
            {
                if (DateTime.Now - cookie.LastSolved < TimeSpan.FromMinutes(2))
                    throw new ApplicationException("Challenge solving fails repeatedly (endless cycle detected)");

                var html = new HtmlDocument();
                html.Load(challengeResponse.GetResponseStream());

                var answer = CalculateAnswer(uri, html);
                var completionUrlBuilder = new UriBuilder(
                    new Uri(uri, html.DocumentNode.SelectSingleNode("//form").GetAttributeValue("action", null)))
                {
                    Query = string.Join("&", html.DocumentNode.SelectNodes("//input")
                        .Select(x => new
                        {
                            Key = x.GetAttributeValue("name", null),
                            Value = x.GetAttributeValue("value", null)
                        })
                        .Select(x => new
                        {
                            x.Key,
                            Value = x.Key == "jschl_answer"
                                ? answer.ToString()
                                : x.Value
                        })
                        .Select(x => $"{x.Key}={Uri.EscapeDataString(x.Value)}"))
                };

                try
                {
                    var solverClient = new SolverWebClient {CookieContainer = sourceCookies};
                    solverClient.DownloadStringAsync(completionUrlBuilder.Uri, new Dictionary<string, string>
                        {
                            ["Referer"] = uri.ToString()
                        })
                        .GetAwaiter().GetResult();
                }
                catch (WebException wex) when (wex.Status == WebExceptionStatus.ProtocolError
                                                && ((HttpWebResponse) wex.Response).StatusCode == HttpStatusCode.Found)
                {
                    //OK, challenge solved (WebClient treats HTTP status 302 as error)
                }              
                var newCookies = sourceCookies.GetCookies(completionUrlBuilder.Uri);
                if (string.IsNullOrWhiteSpace(newCookies[ClearanceCookieName]?.Value))
                    throw new ApplicationException("Something went wrong, failed to receive the clearance cookie");
                cookie.Id = newCookies[IdCookieName];
                cookie.Clearance = newCookies[ClearanceCookieName];
                cookie.LastSolved = DateTime.Now;
                m_ClearanceCookieStorage.StoreCookie(uri, cookie);
                return cookie;
            }
        }

        private static long CalculateAnswer(Uri sourceUri, HtmlDocument html)
        {
            var scriptNode = html.DocumentNode.SelectSingleNode("//script[not(@src)]");
            if (scriptNode == null)
                throw new ApplicationException("Challenge script not detected, this is an ordinary 503 - Service Unavailable error");
            var result = new Engine(x =>
                {
                    x.TimeoutInterval(TimeSpan.FromSeconds(2));
                    x.Strict(false);
                })
                .Execute(M_InitJs)
                .Execute($"setSourceUri(\"{new Uri(sourceUri.GetLeftPart(UriPartial.Authority))}\");")
                .Execute(scriptNode.InnerText);

            var answer = (long) result.Execute("getAnswer();")
                .GetCompletionValue()
                .AsNumber();
            // Limit timeout to 0.1..10 secs in case of some script error
            Thread.Sleep(Math.Max(100, Math.Min(10000, (int) result.GetValue("timeout").AsNumber())));
            return answer;
        }
    }
}
