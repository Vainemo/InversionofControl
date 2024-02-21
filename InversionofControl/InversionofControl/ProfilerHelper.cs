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
}
