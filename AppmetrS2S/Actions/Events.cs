using System;

namespace AppmetrS2S.Actions
{
    public static class Events
    {
        public static Event ServerInstall()
        {
            AppMetrAction event = new Event("server/server_install");
            
            return event;
        }
    }
}
