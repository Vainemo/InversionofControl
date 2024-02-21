using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public static class Guard
    {
        // [return:NotNull]:用于在方法的返回值上指示该返回值不应为 null。它是用于进行静态代码分析和编译时验证的一种方式，以确保方法不会返回 null 值。
        [return:NotNull]
        public static T NotNull<T>([NotNull] T? value, [CallerArgumentExpression(nameof(value))] string? paramName=default)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(value, paramName);
#else
            if(t is null)
            {
              throw new ArgumentNullExcetion(paramName);
            }
#endif
            return value;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str"></param>
        /// <param name="paramName"></param>
        /// <returns></returns>
        [return: NotNull]
        public static string NotNullOrEmpty([NotNull] string? str, [CallerArgumentExpression(nameof(str))] string? paramName=null)
        {
            /*CallerArgumentExpression特性:
             * 用于在调用方法时获取参数的调用表达式。它可以帮助在编译时获得参数的名称和表达式，用于调试和错误处理。
             * 如果传入的 str 参数为 null，则抛出 ArgumentNullException 异常。在异常中，我们使用了传递给特性的参数名称 str，它是在编译时生成的调用表达式的一部分。
            */
#if NET7_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(str, paramName);
#else
            NotNull(str,paramName)
            if(str.Length==0)
            {
               Throw new ArgumentException("The argument can not be Empty",paramName);
            }
#endif
            return str;
        }
        public static T Ensure<T>(Func<T, bool> condition, T t, [CallerArgumentExpression(nameof(t))] string? paramName = null)
        {
            NotNull(condition);
            if (!condition(t))
            {
                throw new ArgumentNullException("the argument does not meet condition", paramName);
            }
            return t;
        }
        public static async Task<T> EnsureAsync<T>(Func<T,Task<bool>> condition,T t, [CallerArgumentExpression(nameof(t))] string? paramName=null)
        {
            NotNull(condition);
            if (! await condition(t))
            {
                throw new ArgumentException("The collection could not be empty", paramName);

            }
            return t;

        }
#if ValueTaskSupport
         public static async Task<T> EnsureAsync<T>(Func<T,ValueTask<bool>> condition,T t, [CallerArgumentExpression(nameof(t))] string? paramName = null)
        {
            NotNull(condition);
            if (! await condition(t))
            {
                throw new ArgumentException("The argument does not mee condition", paramName);
            }
            return t;
        }
#endif
    }
}
