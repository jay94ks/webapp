using System;
using System.Collections.Generic;

namespace WebApp.Extensions
{
    public static class ObjectExtension
    {
        /// <summary>
        /// Get types of given objects.
        /// </summary>
        /// <param name="This"></param>
        /// <returns></returns>
        public static Type[] GetTypes(this IReadOnlyCollection<object> This)
        {
            List<Type> Types = new List<Type>();

            foreach (var Each in This)
                Types.Add(Each.GetType());

            return Types.ToArray();
        }
    }
}
