using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static InversionofControl.CacheUtil;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public static class ReflectionExtension
    {
        /// <summary>
        /// 发现方法的属性并提供对方法元数据的访问(获得对象类型的方法)
        /// </summary>
        /// <param name="type"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public static MethodInfo? GetMethodBySignature(this Type type, MethodInfo method)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(x => x.Name.Equals(method)).ToArray();
            var parameterTypes = method.GetParameters().Select(x => x.ParameterType).ToArray();
            if (method.ContainsGenericParameters)
            {
                foreach (var info in methods)
                {
                    var innerParams = info.GetParameters();
                    if (innerParams.Length != parameterTypes.Length)
                    {
                        continue;
                    }
                    var idx = 0;
                    foreach (var param in innerParams)
                    {
                        if (!param.ParameterType.IsGenericParameter && !parameterTypes[idx].IsGenericParameter && param.ParameterType != parameterTypes[idx])
                        {
                            break;
                        }
                        idx++;
                    }
                    if (idx < parameterTypes.Length)
                    {
                        continue;
                    }
                    return info;
                }
                return null;

            }
            var baseMethod = type.GetMethod(method.Name, parameterTypes);
            return baseMethod;
        }
        public static MethodInfo? GetBaseMethod(this MethodInfo? currentMethod)
        {
            if (null == currentMethod?.DeclaringType?.BaseType)
            {
                return null;
            }
            return currentMethod.DeclaringType.BaseType.GetMethodBySignature(currentMethod);
        }
        public static bool IsVisibleAndVirtual(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return (property.CanRead && property.GetMethod!.IsVisibleAndVirtual()) || (property.CanWrite && property.SetMethod!.IsVisibleAndVirtual());
        }
        /// <summary>
        /// 获取该类的方法是否是可见的虚方法
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static bool IsVisibleAndVirtual(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentException(nameof(method));
            }
            //IsFinal:该方法不可被重写
            if (method.IsStatic || method.IsFinal)
            {
                return false;
            }
            return method.IsVirtual && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }
        /// <summary>
        /// 获取该类的方法是否可见
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsVisible(this MethodBase method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            //IsFamily:此方法或构造函数仅在其类和派生类内可见。
            //IsFamilyOrAssembly:此方法或构造函数可由派生类（无论其位置如何）和同一程序集中的类调用
            return method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly;
        }
        /// <summary>
        /// 一个对象扩展方法，如果 DisplayAttribute 不存在，则获取 DisplayName，返回 MemberName
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        //@this :转义机制，允许在标识符前添加 @ 符号来将其视为普通标识符，而不是关键字。这样，你就可以在代码中使用类似于 @this、@class、@event 等标识符，而不会产生编译错误
        public static string GetDisplayName(this MemberInfo @this)
        {
            return @this.GetCustomAttribute<DisplayNameAttribute>()?.DisplayName ?? @this.GetCustomAttribute<DisplayAttribute>()?.Name ?? @this.Name;
        }
        public static string GetColumnName(this PropertyInfo proertyInfo)
        {
            //ColumnAttribute:用于定义对象属性与数据库表列之间的映射关系,ColumnAttribute允许你在数据对象或实体类的属性上标记，以指定该属性在数据库表中对应的列名，从而实现对象与数据库表的映射。
            return proertyInfo.GetCustomAttribute<ColumnAttribute>()?.Name ?? proertyInfo.Name;
        }
        /// <summary>
        /// 用于搜索特定名称的字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FieldInfo? GetField<T>(this T @this, string name)
        {
            //FieldInfo:发现字段的属性并提供对字段元数据的访问权限。
            return CacheUtil.GetTypeFields(typeof(T)).FirstOrDefault(x => x.Name == name);
        }
        /// <summary>
        /// 获取特定访问权限的字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <param name="bindingFlags"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static FieldInfo? GetField<T>(this T @this, string name, BindingFlags bindingFlags)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }
            return @this.GetType().GetField(name, bindingFlags);
        }
        public static FieldInfo[] GetFields(this object @this)
        {
            return CacheUtil.GetTypeFields(Guard.NotNull(@this, nameof(@this)).GetType());
        }
        public static FieldInfo[] GetFields(this object @this, BindingFlags bindingFlags)
        {
            return @this.GetType().GetFields(bindingFlags);
        }
        /// <summary>
        /// 根据字段名获取该字段的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="fieldName"></param>
        /// <returns></returns>
        public static object? GetFieldValue<T>(this T @this, string fieldName)
        {
            var field = @this.GetField(fieldName);
            return field?.GetValue(@this);
        }
        public static MethodInfo? GetMeThod<T>(this T @this, string name)
        {
            return CacheUtil.TypeMethodCache.GetOrAdd(typeof(T), t => t.GetMethods()).First(x => x.Name == name);
        }
        public static MethodInfo? GetMethod<T>(this T @this, string name, BindingFlags bindIngAttr)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }
            return @this.GetType().GetMethod(name, bindIngAttr);
        }
        public static MethodInfo[] GetMethods<T>(this T @this)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return CacheUtil.TypeMethodCache.GetOrAdd(@this.GetType(), t => t.GetMethods());
        }
        /// <summary>
        /// 获取T类指定访问权限的所有方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static MethodInfo[] GetMethods<T>(this T @this, BindingFlags bindingAttr)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.GetType().GetMethods(bindingAttr);
        }
        /// <summary>
        /// 获取指定类的所有属性
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(this object @this)
        {
            return CacheUtil.GetTypeProperties(@this.GetType());
        }
        /// <summary>
        /// 获取指定类的同一访问权限的所有属性
        /// </summary>
        /// <param name="this"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetProperties(this object @this, BindingFlags bindingAttr)
        {
            return @this.GetType().GetProperties(bindingAttr);
        }
        /// <summary>
        /// 根据名称获取T类对应属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static PropertyInfo? GetProperty<T>(this T @this, string name)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return CacheUtil.GetTypeProperties(@this.GetType()).FirstOrDefault(_ => _.Name == name);
        }
        /// <summary>
        /// 根据名称和访问权限获取T类的对应属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <param name="bindingAttr"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static PropertyInfo? GetProperty<T>(this T @this, string name, BindingFlags bindingAttr)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            return @this.GetType().GetProperty(name, bindingAttr);
        }
        /// <summary>
        /// 通过名称获取T类的属性或者字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static MemberInfo? GetPropertyOrField<T>(this T @this, string name)
        {
            //MemberInfo:MemberInfo类代表类型成员（如字段、属性、方法、构造函数等）的抽象基类
            //MemberInfo 类提供了用于访问类型成员元数据和属性的功能，但本身是一个抽象类，不能直接实例化。
            //其派生类包括 FieldInfo、PropertyInfo、MethodInfo、ConstructorInfo 等，分别用于表示字段、属性、方法、构造函数等成员。
            var property = @this.GetProperty(name);
            if (property != null)
            {
                return property;
            }

            var field = @this.GetField(name);
            return field;
        }
        /// <summary>
        /// 获得T类对应属性名称的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static object? GetPropertyValue<T>(this T @this, string propertyName)
        {
            var property = @this.GetProperty(propertyName);
            return property?.GetValueGetter<T>()?.Invoke(@this);
        }
        /// <summary>
        /// 判断该类是否应用了attributeType特性
        /// </summary>
        /// <param name="this"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool IsAttributeDefined(this object @this, Type attributeType, bool innert = true)
        {
            //MemberInfo.IsDefined(Type, Boolean) 方法是用于检查成员（如字段、属性、方法、构造函数等）
            //是否应用了指定类型的自定义特性的方法
            return @this.GetType().IsDefined(attributeType, innert);
        }
        /// <summary>
        /// 设置对应类的是类对象fieldName字段的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="fieldName"></param>
        /// <param name="value"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void SetFieldValue<T>(this T @this, string fieldName, object? value)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var type = @this.GetType();
            //搜索所有公共和非公共、实例和静态字段。
            var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            field?.SetValue(@this, value);
        }
        /// <summary>
        /// 设置T类名称为propertyName属性的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        public static void SetPropertyValue<T>(this T @this, string propertyName, object? value) where T : class
        {
            var property = @this.GetProperty(propertyName);
            property?.GetValueSetter()?.Invoke(@this, value);
        }
        /// <summary>
        /// 获取该属性Get访问器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static Func<T, object?>? GetValueGetter<T>(this PropertyInfo propertyInfo)
        {
            return StrongTypedCache<T>.PropertyValueGetters.GetOrAdd(propertyInfo, prop =>
            {
                if (!prop.CanRead)
                    return null;

                var instance = Expression.Parameter(typeof(T), "i");
                var property = Expression.Property(instance, prop);
                var convert = Expression.TypeAs(property, typeof(object));
                return (Func<T, object>)Expression.Lambda(convert, instance).Compile();
            });
        }
        public static Func<object, object?>? GetValueGetter(this PropertyInfo propertyInfo)
        {
            return CacheUtil.PropertyValueGetters.GetOrAdd(propertyInfo, prop =>
            {
                if (!prop.CanRead)
                    return null;
                Debug.Assert(propertyInfo.DeclaringType != null);
                var instance = Expression.Parameter(typeof(object), "obj");
                var getterCall = Expression.Call(propertyInfo.DeclaringType!.IsValueType
                    ? Expression.Unbox(instance, propertyInfo.DeclaringType)
                    : Expression.Convert(instance, propertyInfo.DeclaringType), prop.GetGetMethod()!);
                var castToObject = Expression.Convert(getterCall, typeof(object));
                return (Func<object, object>)Expression.Lambda(castToObject, instance).Compile();
            });
        }
        /// <summary>
        /// 获取类型 T 的属性的设置器（Setter）并返回对应的 Action 委托。方法允许通过属性的 PropertyInfo 对象，
        /// 动态创建一个用于设置属性值的委托，并将其缓存起来。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyInfo"></param>
        /// <returns></returns>
        public static Action<T, object?>? GetValueSetter<T>(this PropertyInfo propertyInfo) where T : class
        {
            return StrongTypedCache<T>.PropertyValueSetters.GetOrAdd(propertyInfo, prop =>
            {
                if (!prop.CanWrite)
                    return null;
                // Expression.Parameter:用于创建表示表达式参数的 ParameterExpression 对象。ParameterExpression 是表示表达式中的参数的类
                //创建一个类型为T,名称为i的ParameterExpression 对象
                //ParameterExpression: 用于表示一个参数，可以是方法的参数、Lambda 表达式的参数，或者其他需要参数的上下文。它通常用于创建表达式树的参数节点。
                var instance = Expression.Parameter(typeof(T), "i");
                var argument = Expression.Parameter(typeof(object), "a");
                //prop.GetSetMethod()! 是用于获取属性的 Setter 方法的反射信息。!为C#8.0新语法表示 null 条件运算符
                //，它的作用是在调用属性或方法之前先进行空引用检查，以避免出现空引用异常 (NullReferenceException)
                var setterCall = Expression.Call(instance, prop.GetSetMethod()!, Expression.Convert(argument, prop.PropertyType));
                //Compile():生成表示 lambda 表达式的委托。
                return (Action<T, object?>)Expression.Lambda(setterCall, instance, argument).Compile();
            });
        }
        public static Action<object, object?>? GetValueSetter(this PropertyInfo propertyInfo)
        {
            Guard.NotNull(propertyInfo, nameof(propertyInfo));
            return CacheUtil.PropertyValueSetters.GetOrAdd(propertyInfo, prop =>
            {
                if (!prop.CanWrite)
                    return null;

                var obj = Expression.Parameter(typeof(object), "o");
                var value = Expression.Parameter(typeof(object));
                var expr =
                Expression.Lambda<Action<object, object?>>(
                    Expression.Call(
                        propertyInfo.DeclaringType!.IsValueType
                            ? Expression.Unbox(obj, propertyInfo.DeclaringType)
                            : Expression.Convert(obj, propertyInfo.DeclaringType),
                        propertyInfo.GetSetMethod()!,
                        Expression.Convert(value, propertyInfo.PropertyType)),
                    obj, value);
                return expr.Compile();
            });
        }
        /// <summary>
        /// 是否存在无参的公共构造函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool HasEmptyConstructor<T>(this T @this)
        {
           return typeof(T).HasEmptyConstructor();
        }
        /// <summary>
        /// 是否是元组类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsValueTuple<T>(this T @this)
        {
            return typeof(T).IsValueTuple();
        }
        /// <summary>
        /// 是否是值类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsValueType<T>(this T @this)
        {
            return typeof(T).IsValueType();
        }
        /// <summary>
        /// 是否是数组类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsArray<T>(this T @this)
        {
            if (@this==null)
            {
                throw new ArgumentNullException(nameof(@this));
            }
            return @this.GetType().IsArray;
        }
        /// <summary>
        /// 是否是枚举类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <returns></returns>
        public static bool IsEnum<T>(this T @this)
        {
            return typeof(T).IsEnum;
        }
        /// <summary>
        /// 检查T是否为type的子类
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="this"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsSubclassOf<T>(this T @this,Type type)
        {
            return typeof(T).IsSubclassOf(type);
        }
        /// <summary>
        /// 获取目标指定类型的自定义特性及是否搜索其父类
        /// </summary>
        /// <param name="element">表示一个程序集</param>
        /// <param name="attributeType"></param>
        /// <param name="innert"></param>
        /// <returns></returns>
        public static Attribute? GetCustomAttribute(this Assembly element,Type attributeType,bool innert)
        {
            return Attribute.GetCustomAttribute(element, attributeType, innert);
        }
        /// <summary>
        /// 获取指定类型的全部自定义特性
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static Attribute[] GetCustomAttributes(this Assembly element,Type attributeType)
        {
            return Attribute.GetCustomAttributes(element, attributeType);
        }
        /// <summary>
        /// 获取目标类型(可包含父类)的全部自定义特性
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static Attribute[] GetCustomAttributes(this Assembly element, Type attributeType, bool inherit)
        {
            return Attribute.GetCustomAttributes(element, attributeType, inherit);
        }
        /// <summary>
        /// 确定是否将指定类型的任何自定义属性应用于程序集
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeType"></param>
        /// <returns></returns>
        public static bool IsDefined(this Assembly element, Type attributeType)
        {
            return Attribute.IsDefined(element, attributeType);
        }
        /// <summary>
        /// 检查给定程序集上是否应用了指定特性
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attributeType"></param>
        /// <param name="inherit"></param>
        /// <returns></returns>
        public static bool IsDefined(this Assembly element, Type attributeType, bool inherit)
        {
            return Attribute.IsDefined(element, attributeType, inherit);
        }
    }
}
