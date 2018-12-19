namespace SupportManager.DAL
{
    public class ApiKey : Entity
    {
        public virtual User User { get; set; }
        public virtual int UserId { get; set; }
        public virtual string Value { get; set; }
        public virtual string CallbackUrl { get; set; }
    }
}