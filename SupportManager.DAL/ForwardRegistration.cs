namespace SupportManager.DAL
{
    public class ForwardRegistration : Entity
    {
        public virtual User ExecutingUser { get; set; }
        public virtual UserPhoneNumber PhoneNumber { get; set; }
    }
}
