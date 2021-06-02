using System;
using System.Reflection;

namespace WebApp.Extensions
{
    public static class TypeExtension
    {
        /// <summary>
        /// Instantiate this type.
        /// </summary>
        /// <typeparam name="BaseType"></typeparam>
        /// <param name="This"></param>
        /// <returns></returns>
        public static BaseType Instantiate<BaseType>(this Type This, params object[] Arguments)
        {
            ConstructorInfo Ctor;
            object Instance;

            if (This != typeof(BaseType) &&
               !This.IsSubclassOf(typeof(BaseType)))
            {
                throw new ArgumentException("BaseType isn't compatible with this type.");
            }

            if (Arguments.Length <= 0)
                 Ctor = This.GetConstructor(Type.EmptyTypes);
            else Ctor = This.GetConstructor(Arguments.GetTypes());

            if (Ctor is null)
                throw new ArgumentException("This type doesn't have constructor which get given arguments.");

            if ((Instance = Ctor.Invoke(Arguments)) is null)
                throw new InvalidProgramException("Fatal: .NET can't create a new instance.");

            return (BaseType) Instance;
        }
    }
}
