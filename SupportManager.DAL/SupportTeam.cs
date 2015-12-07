using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SupportManager.DAL
{
    public class SupportTeam : Entity
    {
        [Required]
        public virtual string Name { get; set; }
        public virtual string PhoneComPort { get; set; }
        public virtual ICollection<TeamMember> Members { get; set; }
    }
}
