using System;
using System.Collections.Generic;
using System.Linq;

namespace Application.Common.Extensions
{
    public static class EnumCache<T> where T : Enum
    {
        private static readonly Dictionary<T, string> NamingCache;
        private static readonly Dictionary<T, string> ValueCache;

        static EnumCache()
        {
            NamingCache = Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(x => x, x => x.ToString());

            ValueCache = Enum.GetValues(typeof(T))
                .Cast<T>()
                .ToDictionary(x => x, x => x.ToString("D"));
        }

        public static string ValueToString(T value)
        {
            return ValueCache[value];
        }

        public static string ToStringFast(T value)
        {
            return NamingCache[value];
        }
    }

    public static class EnumCacheExtensions
    {
        public static string ToStringFast<T>(this T @enum)
            where T : Enum
        {
            return EnumCache<T>.ToStringFast(@enum);
        }

        public static string ValueToString<T>(this T @enum)
            where T : Enum
        {
            return EnumCache<T>.ValueToString(@enum);
        }
    }
}
