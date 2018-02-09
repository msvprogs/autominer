using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Msv.AutoMiner.Common.External.Contracts;
using Msv.BrowserCheckBypassing;
using Msv.HttpTools;
using Msv.HttpTools.Contracts;
using NLog;

namespace Msv.AutoMiner.Common.External
{
    public class LoggedWebClient : IWebClient
    {
        // Do not use the GetCurrentClassLogger() method - logger name will be obfuscated
        private static readonly ILogger M_Logger = LogManager.GetLogger(nameof(LoggedWebClient));

        public virtual string DownloadString(
            string url, Encoding encoding = null, Dictionary<string, string> headers = null)
        {
            var webClient = CreateBaseWebClient();         
            if (encoding != null)
                webClient.Encoding = encoding;
            M_Logger.Debug($"GET {url}...");
            var response = webClient.DownloadStringAsync(new Uri(url), headers ?? new Dictionary<string, string>())
                .GetAwaiter().GetResult();
            M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
            return response;          
        }

        public string DownloadString(Uri url, Encoding encoding = null)
            => DownloadString(url.ToString(), encoding);

        public string DownloadString(string url, Dictionary<string, string> headers)
        {
            var webClient = CreateBaseWebClient();  
            M_Logger.Debug($"GET {url}...");
            var response = webClient.DownloadStringAsync(new Uri(url), headers).GetAwaiter().GetResult();
            M_Logger.Debug($"GET {url} response:{Environment.NewLine}{response}");
            return response;          
        }

        public string UploadString(string url, string data, Dictionary<string, string> headers, NetworkCredential credentials = null, string contentType = null)
        {
            var webClient = CreateBaseWebClient();           
            M_Logger.Debug($"POST {url}{Environment.NewLine}{data}");
            var response = webClient.UploadStringAsync(new Uri(url), data, headers, credentials, contentType).GetAwaiter().GetResult();
            M_Logger.Debug($"POST {url} response:{Environment.NewLine}{response}");
            return response;            
        }

        protected virtual IBaseWebClient CreateBaseWebClient()
            => new BrowserCheckBypassingWebClient(
                new CorrectWebClient(),
                new BrowserCheckBypasserFactory(MemoryClearanceCookieStorage.Instance),
                MemoryClearanceCookieStorage.Instance);
     }
}