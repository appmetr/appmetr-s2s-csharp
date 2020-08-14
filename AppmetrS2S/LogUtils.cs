using System;
using Common.Logging;

namespace AppmetrS2S
{
    public static class LogUtils
    {
        public static ILog CustomLog { get; set; }

        public static ILog GetLogger(Type type)
        {
            if (CustomLog != null)
                return CustomLog;

            return LogManager.GetLogger(type);
        }
    }
}
