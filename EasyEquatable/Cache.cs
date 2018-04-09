using System;
using System.Linq.Expressions;

namespace EasyEquatable
{
    public static class Cache<T>
    {
        internal static object _lockObject = new object();
        internal static Func<T, T, bool> DelegateEntry;
        public static Expression<Func<T, T, bool>> ExpressionEntry { get; internal set; }
    }
}
