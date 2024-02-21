using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InversionofControl
{
    /// <summary>
    /// 用户配置
    /// </summary>
    public interface IProfiler
    {
        void Start();
        void Stop();
        void Restart();
        TimeSpan Elapsed { get; }

    }
    public class StopwatchProfiler : IProfiler
    {
        private readonly Stopwatch _stopwatch;
        /// <summary>
        /// 获取当前实例测量得出的总运行时间。
        /// </summary>
        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public StopwatchProfiler()
        {
            _stopwatch = new Stopwatch();
        }
        public void Restart()
        {
            _stopwatch.Restart();
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }
    }
}
