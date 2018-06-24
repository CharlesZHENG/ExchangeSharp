using System;
using System.Collections.Generic;
using System.Text;

namespace ExchangeSharp.Utility
{
    public static class TimeHelper
    {
        public static long GetTotalMilliseconds()
        {
            DateTime timeStamp = new DateTime(1970, 1, 1);
            long stamp = (DateTime.UtcNow.Ticks - timeStamp.Ticks) / 10000;
            return stamp;
        }
    }
}
