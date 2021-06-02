using System;

namespace WebApp.Extensions
{
    public static class ArrayExtension
    {
        /// <summary>
        /// Execute an Action for all elements.
        /// </summary>
        /// <typeparam name="ElemType"></typeparam>
        /// <param name="This"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        public static ElemType[] Each<ElemType>(this ElemType[] This, Action<ElemType> Action)
        {
            if (This != null)
            {
                foreach (var Each in This)
                    Action(Each);
            }

            return This;
        }

        /// <summary>
        /// Execute an Action for all elements.
        /// </summary>
        /// <typeparam name="ElemType"></typeparam>
        /// <param name="This"></param>
        /// <param name="Action"></param>
        /// <returns></returns>
        public static ElemType[] Each<ElemType>(this ElemType[] This, Action<int, ElemType> Action)
        {
            if (This != null)
            {
                for(int i = 0; i < This.Length; ++i)
                    Action(i, This[i]);
            }

            return This;
        }
    }
}
