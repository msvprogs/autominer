using System;
using System.ComponentModel.DataAnnotations;

namespace Msv.AutoMiner.FrontEnd.Infrastructure
{
    public class PoolUrlAttribute : DataTypeAttribute
    {
        public PoolUrlAttribute()
            : base(DataType.Url)
        { }

        public override bool IsValid(object value)
        {
            var urlString = value as string;
            if (string.IsNullOrEmpty(urlString))
                return true;
            if (!Uri.IsWellFormedUriString(urlString, UriKind.Absolute))
                return false;
            var uri = new Uri(urlString, UriKind.Absolute);
            return uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.CurrentCultureIgnoreCase)
                   || uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.CurrentCultureIgnoreCase)
                   || uri.Scheme.Equals("stratum+tcp", StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
