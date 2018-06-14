using System;
using HtmlAgilityPack;
using JetBrains.Annotations;
using Msv.AutoMiner.Common.External.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msv.AutoMiner.Common
{
    public static class WebClientExtensions
    {
        public static dynamic DownloadJsonAsDynamic([NotNull] this IWebClient webClient, [NotNull] Uri url)
            => DownloadJsonAsDynamic(webClient, url.ToString());

        public static dynamic DownloadJsonAsDynamic([NotNull] this IWebClient webClient, [NotNull] string url)
        {
            if (webClient == null) 
                throw new ArgumentNullException(nameof(webClient));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return JsonConvert.DeserializeObject<dynamic>(webClient.DownloadString(url));
        }

        public static T DownloadJsonAs<T>([NotNull] this IWebClient webClient, [NotNull] Uri url)
            => DownloadJsonAs<T>(webClient, url.ToString());

        public static T DownloadJsonAs<T>([NotNull] this IWebClient webClient, [NotNull] string url)
        {
            if (webClient == null) 
                throw new ArgumentNullException(nameof(webClient));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return JsonConvert.DeserializeObject<T>(webClient.DownloadString(url));
        }

        public static JArray DownloadJArray([NotNull] this IWebClient webClient, [NotNull] Uri url)
            => DownloadJArray(webClient, url.ToString());

        public static JArray DownloadJArray([NotNull] this IWebClient webClient, [NotNull] string url)
        {
            if (webClient == null) 
                throw new ArgumentNullException(nameof(webClient));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return JsonConvert.DeserializeObject<JArray>(webClient.DownloadString(url));
        }

        public static JObject DownloadJObject([NotNull] this IWebClient webClient, [NotNull] string url)
        {
            if (webClient == null) 
                throw new ArgumentNullException(nameof(webClient));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            return JsonConvert.DeserializeObject<JObject>(webClient.DownloadString(url));
        }

        public static HtmlDocument DownloadHtml([NotNull] this IWebClient webClient, [NotNull] Uri url)
            => DownloadHtml(webClient, url.ToString());

        public static HtmlDocument DownloadHtml([NotNull] this IWebClient webClient, [NotNull] string url)
        {
            if (webClient == null) 
                throw new ArgumentNullException(nameof(webClient));
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(webClient.DownloadString(url));
            return htmlDocument;
        }
    }
}
