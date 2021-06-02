using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Extensions
{
    public static class HeaderDictionaryExtension
    {
        /// <summary>
        /// Get Header As String with Fail-over.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Header"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static string GetHeader(this IHeaderDictionary This, string Header, string Default = null)
        {
            if (!This.TryGetValue(Header, out var RawValue) || RawValue.Count <= 0)
                return Default;

            string OutValue = string.Join(" ", RawValue).Trim();
            if (OutValue.Length <= 0)
                return Default;

            return OutValue;
        }

        /// <summary>
        /// Get and Parse the header
        /// </summary>
        /// <typeparam name="OutType"></typeparam>
        /// <param name="This"></param>
        /// <param name="Header"></param>
        /// <param name="Parser"></param>
        /// <returns></returns>
        public static OutType GetHeader<OutType>(this IHeaderDictionary This, 
            string Header, OutType Default, Func<string, OutType> Parser)
        {
            if (!This.TryGetValue(Header, out var RawValue) || RawValue.Count <= 0)
                return Default;

            string OutValue = string.Join(" ", RawValue).Trim();
            if (OutValue.Length <= 0)
                return Default;

            return Parser(OutValue);
        }

        /// <summary>
        /// Get Header as HTTP Date.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Header"></param>
        /// <param name="Default"></param>
        /// <returns></returns>
        public static DateTime GetHeader(this IHeaderDictionary This, string Header, DateTime Default)
        {
            if (!This.TryGetValue(Header, out var RawValue) || RawValue.Count <= 0)
                return Default;

            string OutValue = string.Join(" ", RawValue).Trim();
            if (OutValue.Length <= 0)
                return Default;

            if (!DateTime.TryParseExact(
                OutValue, "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                CultureInfo.InvariantCulture.DateTimeFormat,
                DateTimeStyles.AssumeUniversal, out var Value))
                return Default;

            return Value;
        }
    }
}
