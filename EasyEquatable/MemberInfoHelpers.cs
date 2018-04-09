using System;
using System.Linq;
using System.Reflection;

namespace EasyEquatable
{
    internal static class MemberInfoHelpers
    {
        /// <summary>
        /// Performs a safe Generic-Type check against a memberinfo.
        /// IOW, This prevents EvilDog : IEasyEquatable(Of Cat)
        /// </summary>
        /// <param name="element"></param>
        /// <param name="openGenericType"></param>
        /// <returns></returns>
        internal static bool implementsGenericInterfaceSafe(MemberInfo element, Type openGenericType)
        {
            Type ourType = getType(element);


            var implementedInterfaces = ourType.GetInterfaces();
            foreach (var interfaceType in implementedInterfaces)
            {

                if (false == interfaceType.IsGenericType) { continue; }
                var genericType = interfaceType.GetGenericTypeDefinition();
                if (genericType == openGenericType)
                {
                    if (interfaceType.GetGenericArguments().Any(q => q == ourType))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static Type getType(MemberInfo element)
        {
            if (element is PropertyInfo)
            {
                return (element as PropertyInfo).PropertyType;
            }
            else if (element is FieldInfo)
            {
                return (element as FieldInfo).FieldType;
            }
            else
            {
                return null;
            }
        }
        internal static bool isType<T>(MemberInfo element)
        {
            var ourType = getType(element);
            return ourType == typeof(T);
        }
    }
}
