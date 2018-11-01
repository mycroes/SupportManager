namespace SupportManager.Telegram.DAL
{
    public class User
    {
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public string ApiKey { get; set; }
        public int? DefaultTeamId { get; set; }
    }
}