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
                return ProfilerHelper.GetElapsedTime
            }
        }
    }
}
