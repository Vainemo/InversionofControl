using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 
    /// </summary>
    public class ProfilerStopper:IDisposable
    {
        private readonly IProfiler _profiler;
        //Action<T>封装一个方法，该方法只有一个参数并且不返回值
        private readonly Action<TimeSpan> _profileAction;
        public ProfilerStopper(IProfiler profiler,Action<TimeSpan> action)
        {

            _profileAction = action?? throw new ArgumentNullException(nameof(action));
            //ArgumentNullException(nameof(profiler));:此构造函数将 Message 新实例的 属性初始化为描述错误的系统提供的消息
            _profiler = profiler??throw new ArgumentNullException(nameof(profiler));
        }
        public void Dispose()
        {
            _profiler.Stop();
            _profileAction(_profiler.Elapsed);
        }
    }
    public sealed class StopwatchStopper : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly Action<TimeSpan> _profileAction;
        public StopwatchStopper(Stopwatch stopwatch,Action<TimeSpan> prfileAction)
        {
            _stopwatch=stopwatch??throw new ArgumentNullException(nameof(stopwatch));
            _profileAction=prfileAction??throw new ArgumentNullException(nameof(_profileAction));
        }
        public void Dispose()
        {
            _stopwatch.Stop();
            _profileAction(_stopwatch.Elapsed);
        }
    }
    public static class ProfileHelper
    {
        public static StopwatchStopper Profile(this Stopwatch watch,Action<TimeSpan> profilerAction)
        {
            Guard.NotNull(watch, nameof(watch)).Restart();
            return new StopwatchStopper(watch, profilerAction);
        }
        //Stopwatch.Frequency（表示每秒的计时周期数）
        public static readonly double TicksPerTimestamp = TimeSpan.TicksPerMillisecond / (double)Stopwatch.Frequency;
        public static TimeSpan GetElapsedTime(long startTimestamp)
        {
#if NET7_0_OR_GREATER
          return  Stopwatch.GetElapsedTime(startTimestamp);
#else
         return   GetElapsedTime(startTimestamp,Stopwatch.GetTimestamp());
#endif
        }
        public static TimeSpan  GetElapsedTime( long startTimestamp, long endTimestamp)
        {
#if NET7_0_OR_GREATER
            return Stopwatch.GetElapsedTime(startTimestamp, endTimestamp);
#else
            
            var ticks = (long)((endTimestamp - startTimestamp) * TicksPerTimestamp);
            return new TimeSpan(ticks);
#endif
        }
    }
}