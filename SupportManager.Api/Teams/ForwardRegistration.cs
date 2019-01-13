using System;
using SupportManager.Api.Users;

namespace SupportManager.Api.Teams
{
    public class ForwardRegistration
    {
        public int Id { get; set; }
        public User User { get; set; }
        public PhoneNumber PhoneNumber { get; set; }
        public DateTimeOffset When { get; set; }
    }
}