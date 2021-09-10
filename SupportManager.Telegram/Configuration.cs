using System;

namespace SupportManager.Telegram
{
    public class Configuration
    {
        public string DbFileName { get; set; }
        public string BotKey { get; set; }
        public Uri SupportManagerUri { get; set; }
        public Uri HostUri { get; set; }
    }
}