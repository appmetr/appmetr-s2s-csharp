namespace AppmetrS2S.Actions
{
	public static class Events
    {
        public static Event ServerInstall(string userId)
        {
            var _event = new Event("server/server_install");
            _event.SetUserId(userId);
            return _event;
        }
    }
}
