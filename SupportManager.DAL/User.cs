using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupportManager.DAL
{
    public class User : Entity
    {
        public virtual string DisplayName { get; set; }
        [Required]
        public virtual string Login { get; set; }
        public virtual ICollection<UserEmailAddress> EmailAddresses { get; set; }
        public virtual ICollection<UserPhoneNumber> PhoneNumbers { get; set; }
        public virtual ICollection<ApiKey> ApiKeys { get; set; }
        public virtual UserEmailAddress PrimaryEmailAddress { get; set; }
        public virtual UserPhoneNumber PrimaryPhoneNumber { get; set; }
    }
}