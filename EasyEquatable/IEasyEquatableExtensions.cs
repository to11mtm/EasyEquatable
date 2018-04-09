using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace EasyEquatable
{
    public static class FunctionMethodBuilder
    {
        public static Func<T,T,bool> Build<T>(Expression<Func<T, T, bool>> expr)
        {
            var fooType = typeof(T);
            var asmName = new AssemblyName(fooType.Name+Guid.NewGuid().ToString());
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var moduleBuilder = asmBuilder.DefineDynamicModule(fooType.Name);
            var typeBuilder = moduleBuilder.DefineType(fooType.Name, TypeAttributes.Public);
            var methodBuilder = typeBuilder.DefineMethod(fooType.Name + "EasyEquatableRuntimeComparer", MethodAttributes.Static, typeof(bool), new[] { fooType });
            expr.CompileToMethod(methodBuilder);
            var createdType = typeBuilder.CreateType();

            var mi = createdType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)[1];
            var func = Delegate.CreateDelegate(typeof(Func<T,T,bool>), mi);
            return (Func<T,T, bool>)func;
        }
    }
    public static class IEasyEquatableExtensions
    {
        public static bool CompareEquatable<T>(this T me, T other) where T : IEquatable<T>
        {
            return me.Equals(other);
        }
        public static bool CompareEasyEquatable<T>(this T me, T other) where T : IEasyEquatable<T>
        {
            Func<T, T, bool> checkExpression = Cache<T>.DelegateEntry;
            if (checkExpression == null)
            {

                var expression = comparisonGenerator<T>();
                lock (Cache<T>._lockObject)
                {
                    //Double check that it wasn't put in during lock.
                    checkExpression = Cache<T>.DelegateEntry;
                    if (checkExpression == null)
                    {
                        Cache<T>.ExpressionEntry = expression;

#if USE_METHODBUILDER
                        Cache<T>.DelegateEntry = FunctionMethodBuilder.Build(expression);                        
#else
                        Cache<T>.DelegateEntry = expression.Compile();
#endif
                    }
                }
            }
            return Cache<T>.DelegateEntry(me, other);
        }



        public static Expression<Func<T, T, bool>> comparisonGenerator<T>()
        {
            var ourType = typeof(T);
            var members = ourType.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.CustomAttributes.Any(ca => ca.AttributeType == typeof(EquatableItemAttribute)));


            var exprList = new List<BinaryExpression>(members.Count());

            var param1 = Expression.Parameter(ourType);
            var param2 = Expression.Parameter(ourType);
            foreach (MemberInfo element in members)
            {
                BinaryExpression eq1 = null;
                var accessor1 = Expression.PropertyOrField(param1, element.Name);
                var accessor2 = Expression.PropertyOrField(param2, element.Name);
                if (isString(element))
                {
                    var compVal = Expression.Constant(element.GetCustomAttribute<EquatableItemAttribute>().StringComparison);
                    var stringCompare = typeof(string).GetMethod("Equals", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(string), typeof(string), typeof(StringComparison) }, null);
                    
                    eq1 = Expression.Equal(Expression.Call(stringCompare, accessor1, accessor2, compVal), Expression.Constant(true));
                }
                else if (isEasyEquatable(element))
                {
                    //Call our EasyEquatable method.
                    eq1 = Expression.Equal(Expression.Call(typeof(IEasyEquatableExtensions), "CompareEasyEquatable", new[] { MemberInfoHelpers.getType(element) }, accessor1, accessor2), Expression.Constant(true));
                }
                else if (isEquatable(element) && !isPrimitive(element))
                {
                    //If here, we have Equatable 
                    //(And aren't dealing with something so simple we should just fall-through.)
                    eq1 = Expression.Equal(Expression.Call(typeof(IEasyEquatableExtensions), "CompareEquatable", new[] { MemberInfoHelpers.getType(element) }, accessor1, accessor2), Expression.Constant(true));
                }

                //Default case: We just do Expression.Equals.
                if (eq1 == null)
                {
                    eq1 = Expression.Equal(accessor1, accessor2);
                }
                exprList.Add(eq1);

            }
            var currentExpr = exprList[0];
            if (exprList.Count > 1)
            {
                for (int i = 1; i < exprList.Count; i++)
                {

                    currentExpr = Expression.AndAlso(currentExpr, exprList[i]);
                }

            }
            var compFunc = Expression.Lambda<Func<T, T, bool>>(currentExpr, param1, param2);

            return compFunc;
        }

        
        private static bool isEquatable(MemberInfo element)
        {
            return MemberInfoHelpers.implementsGenericInterfaceSafe(element, typeof(IEquatable<>));
        }
        private static bool isEasyEquatable(MemberInfo element)
        {
            return MemberInfoHelpers.implementsGenericInterfaceSafe(element, typeof(IEasyEquatable<>));
            /*Type ourType = getType(element);


            var implementedInterfaces = ourType.GetInterfaces();
            foreach (var interfaceType in implementedInterfaces)
            {

                if (false == interfaceType.IsGenericType) { continue; }
                var genericType = interfaceType.GetGenericTypeDefinition();
                if (genericType == typeof(IEasyEquatable<>))
                {
                    if (interfaceType.GetGenericArguments().Any(q => q == ourType))
                    {
                        return true;
                    }
                }
            }
            return false;*/
        }

        private static bool isString(MemberInfo element)
        {
            return MemberInfoHelpers.isType<string>(element);
        }

        private static bool isPrimitive(MemberInfo element)
        {
            return MemberInfoHelpers.getType(element).IsPrimitive;
        }
    }
}
