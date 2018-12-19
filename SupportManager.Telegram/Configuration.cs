using System;

namespace SupportManager.Telegram
{
    public class Configuration
    {
        public string BotKey { get; set; }
        public Uri SupportManagerUri { get; set; }
        public Uri HostUri { get; set; }
    }
}