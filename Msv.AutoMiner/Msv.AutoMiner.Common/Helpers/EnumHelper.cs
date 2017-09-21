﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Msv.AutoMiner.Common.Helpers
{
    public static class EnumHelper
    {
        public static IEnumerable<T> GetValues<T>()
            where T : struct
        {
            if (!typeof(T).IsEnum)
                throw new ArgumentOutOfRangeException(nameof(T), "Not an enum type");

            return Enum.GetValues(typeof(T)).Cast<T>();
        }
    }
}
