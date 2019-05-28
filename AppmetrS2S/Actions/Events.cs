using System;

namespace AppmetrS2S.Actions
{
    public static class Events
    {
        public static Event ServerInstall()
        {
            return new Event("server/server_install");
        }
    }
}
