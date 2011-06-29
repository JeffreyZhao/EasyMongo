using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public static class Int64Version
    {
        public static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrent()
        {
            return (long)Math.Floor((DateTime.UtcNow - s_epoch).TotalMilliseconds);
        }
    }

    public static class DateTimeVersion
    {
        public static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime GetCurrent()
        {
            var time = Math.Floor((DateTime.UtcNow - s_epoch).TotalMilliseconds);
            return s_epoch.AddMilliseconds(time);
        }
    }
}
