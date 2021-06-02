using System;
using System.Collections.Generic;
using System.Reflection;

namespace WebApp.Extensions
{
    public static class AssemblyExtension
    {
        /// <summary>
        /// Get types which met the predicate.
        /// </summary>
        /// <param name="This"></param>
        /// <param name="Predicate"></param>
        /// <returns></returns>
        public static Type[] GetTypes(this Assembly This, Predicate<Type> Predicate)
        {
            List<Type> Types = new List<Type>();

            foreach(var Each in This.GetTypes())
            {
                if (Predicate(Each))
                    Types.Add(Each);
            }

            return Types.ToArray();
        }
    }
}
