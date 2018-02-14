using System;
using System.ComponentModel.DataAnnotations;
using Msv.AutoMiner.Common.Data;

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
            return PoolProtocolUriSchemes.HasScheme(uri.Scheme);
        }
    }
}
