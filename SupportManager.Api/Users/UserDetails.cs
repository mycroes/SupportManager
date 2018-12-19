using System.Collections.Generic;

namespace SupportManager.Api.Users
{
    public class UserDetails
    {
        public int Id { get; set; }
        public string DisplayName { get; set; }
        public string Login { get; set; }
        public List<EmailAddress> EmailAddresses { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; }
    }
}
