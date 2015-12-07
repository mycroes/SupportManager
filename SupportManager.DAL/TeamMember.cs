using System.ComponentModel.DataAnnotations;

namespace SupportManager.DAL
{
    public class TeamMember : Entity
    {
        [Required]
        public virtual SupportTeam Team { get; set; }
        [Required]
        public virtual User User { get; set; }
        public virtual bool IsAdministrator { get; set; }
    }
}