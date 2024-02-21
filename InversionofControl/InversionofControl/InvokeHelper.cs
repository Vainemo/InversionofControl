using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public static class InvokeHelper
    {
        public static double Profile(Action action)
        {
            Guard.NotNull(action, nameof(action));
            var stopwatch = ValueStopwatch.StartNew();
            action();
            return stopwatch.Elapsed.TotalMillIseconds;
        }
        public static double Profile<T>(Action<T> action,T t)
        {
            Guard.NotNull(action, nameof(action));
            var stopwatch = ValueStopwatch.StartNew();
            action(t);
            return stopwatch.Elapsed.TotalMillIseconds;
        }
        public static double Profile<T1,T2>(Action<T1,T2> action, T1 t1,T2 t2)
        {
            Guard.NotNull(action, nameof(action));
            var stopwatch = ValueStopwatch.StartNew();
            action(t1,t2);
            return stopwatch.Elapsed.TotalMillIseconds;
        }
        public static double Profile<T1, T2,T3>(Action<T1, T2,T3> action, T1 t1, T2 t2,T3 t3)
        {
            Guard.NotNull(action, nameof(action));
            var stopwatch = ValueStopwatch.StartNew();
            action(t1, t2,t3);
            return stopwatch.Elapsed.TotalMillIseconds;
        }
        public static async Task<double> ProfileAsync(Func<Task> action)
        {
            var stopwatch=ValueStopwatch.StartNew();
            await action();
            return stopwatch.Elapsed.TotalMilliseconds;
        }
    }
   
}
