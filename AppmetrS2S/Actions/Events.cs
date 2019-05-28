namespace AppmetrS2S.Actions
{
    #region using directives

    using System;

    #endregion
    public static class Events
    {
        public static Event ServerInstall(String userId)
        {
            Event _event = new Event("server/server_install");
            _event.SetUserId(userId);
            return _event;
        }
    }
}
