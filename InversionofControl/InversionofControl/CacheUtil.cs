using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 缓存类型
    /// </summary>
    public class CacheUtil
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertyCache=new ConcurrentDictionary<Type, PropertyInfo[]>();
        public static PropertyInfo[] GetTypeProperties(Type type)
        {
            Guard.NotNull(type);
            return TypePropertyCache.GetOrAdd(type,t=>t.GetProperties());
        }
        public static FieldInfo[] GetTypeFields(Type type)
        {
            Guard.NotNull(type);
            return TypeFieldCache.GetOrAdd(type, t =>t.GetFields());
        }
        private static readonly ConcurrentDictionary<Type, FieldInfo[]> TypeFieldCache = new();
        internal static readonly ConcurrentDictionary<Type, MethodInfo[]> TypeMethodCache=new();
        internal static readonly ConcurrentDictionary<Type,Func<ServiceContainer,object>> TypeNewFuncCache=new();
        internal static readonly ConcurrentDictionary<Type,ConstructorInfo?> TypeConstructorCache=new();
        internal static readonly ConcurrentDictionary<Type,Func<object>> TypeEmptyConstructorFuncCache=new();
        internal static readonly ConcurrentDictionary<Type, Func<object?[],object>>TypeConstructorFuncCache=new();
        internal static readonly ConcurrentDictionary<PropertyInfo, Func<object, object?>?> PropertyValueGetters = new();
        internal static readonly ConcurrentDictionary<PropertyInfo,Action<object,object?>?> PropertyValueSetters=new();
        internal static class StrongTypedCache<T>
        {
            public static readonly ConcurrentDictionary<PropertyInfo,Func<T,object?>?> PropertyValueGetters = new();
            public static readonly ConcurrentDictionary<PropertyInfo,Action<T,object?>?> PropertyValueSetters=new(); 
        }
    }
    
}
