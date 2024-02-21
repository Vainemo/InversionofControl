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
    public struct ValueStopwatch
    {
        private long _startTimestamp, _stopTomestamp;
        private ValueStopwatch(long startTimestamp)
        {
            _startTimestamp = startTimestamp;
            _stopTomestamp = 0;
        }
        public TimeSpan Elapsed
        {
            get
            {
                if (_stopTomestamp==0)
                {
                    _stopTomestamp = Stopwatch.GetTimestamp();
                }
                return ProfileHelper.GetElapsedTime(_startTimestamp, _stopTomestamp);
            }
        }
        public bool IsRunning => _stopTomestamp == 0;
        public void Restart()
        {
            _stopTomestamp = 0;
            _startTimestamp=Stopwatch.GetTimestamp();
        }
        public void Stop()
        {
            //获取计时器机制中的当前刻度数
            _stopTomestamp = Stopwatch.GetTimestamp();
        }
        public static ValueStopwatch StartNew() => new(Stopwatch.GetTimestamp());
    }

}
