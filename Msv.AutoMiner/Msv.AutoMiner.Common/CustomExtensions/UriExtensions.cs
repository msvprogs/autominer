using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;

namespace Msv.AutoMiner.Common.CustomExtensions
{
    public static class UriExtensions
    {
        private static readonly Uri M_ExampleUri = new Uri("http://example.com");

        public static IReadOnlyDictionary<string, string> ParseQueryString([NotNull] this Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var queryString = uri.IsAbsoluteUri
                ? uri.Query
                : new Uri(M_ExampleUri, uri).Query;
            var nameValues = HttpUtility.ParseQueryString(queryString);
            return nameValues.AllKeys
                .ToDictionary(x => x, x => nameValues[x]);
        }
    }
}
