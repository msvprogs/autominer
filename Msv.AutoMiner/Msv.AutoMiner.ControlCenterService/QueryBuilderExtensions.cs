using System;
using Microsoft.AspNetCore.Http.Extensions;

namespace Msv.AutoMiner.ControlCenterService
{
    public static class QueryBuilderExtensions
    {
        public static string ToStringWithoutPrefix(this QueryBuilder queryBuilder)
        {
            if (queryBuilder == null) 
                throw new ArgumentNullException(nameof(queryBuilder));

            return queryBuilder.ToString().TrimStart('?');
        }
    }
}
