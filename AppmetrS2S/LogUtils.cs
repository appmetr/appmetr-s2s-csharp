using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using log4net;

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
