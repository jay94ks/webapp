using System;
using System.Collections.Generic;

namespace WebApp.Extensions
{
    public static class ReadOnlyCollectionExtension
    {
        /// <summary>
        /// Map the collection to callback.
        /// </summary>
        /// <typeparam name="InType"></typeparam>
        /// <typeparam name="OutType"></typeparam>
        /// <param name="This"></param>
        /// <param name="Callback"></param>
        /// <returns></returns>
        public static OutType[] Map<InType, OutType>(this IReadOnlyCollection<InType> This, Func<InType, OutType> Callback)
        {
            OutType[] RetVal = new OutType[This.Count];
            int Index = 0;

            foreach (var Each in This)
                RetVal[Index++] = Callback(Each);

            return RetVal;
        }

        /// <summary>
        /// Merge multiple arrays into one.
        /// </summary>
        /// <typeparam name="ElemType"></typeparam>
        /// <param name="This"></param>
        /// <param name="Arrays"></param>
        /// <returns></returns>
        public static ElemType[] Merge<ElemType>(this IReadOnlyCollection<ElemType> This, params IReadOnlyCollection<ElemType>[] Arrays)
        {
            int Total = This != null ? This.Count : 0;
            ElemType[] OutArray; int Index = 0;

            foreach (var Each in Arrays)
                Total += Each != null ? Each.Count : 0;

            OutArray = new ElemType[Total];
            if (This != null)
            {
                foreach (var Item in This)
                    OutArray[Index++] = Item;
            }

            foreach (var Each in Arrays)
            {
                if (Each is null)
                    continue;

                foreach (var Item in Each)
                    OutArray[Index++] = Item;
            }

            return OutArray;
        }
    }
}
