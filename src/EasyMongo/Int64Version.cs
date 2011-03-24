using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyMongo
{
    public static class Int64Version
    {
        private static readonly DateTime s_epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long GetCurrent()
        {
            return (long)(DateTime.UtcNow - s_epoch).TotalMilliseconds;
        }
    }
}
