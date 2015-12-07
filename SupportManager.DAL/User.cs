using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using Microsoft.AspNet.Identity;

namespace SupportManager.DAL
{
    public class User : Entity
    {
        [Required]
        public virtual string Name { get; set; }
        public virtual ICollection<UserEmailAddress> EmailAddresses { get; set; }
        public virtual ICollection<UserPhoneNumber> PhoneNumbers { get; set; }
        public virtual UserEmailAddress PrimaryEmailAddress { get; set; }
        public virtual UserPhoneNumber PrimaryPhoneNumber { get; set; }
    }
}