using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportManager.DAL
{
    public class TeamMember
    {
        public virtual SupportTeam Team { get; set; }
        [Key, ForeignKey(nameof(Team)), Column(Order = 1)]
        public virtual int TeamId { get; set; }
        public virtual User User { get; set; }
        [Key, ForeignKey(nameof(User)), Column(Order = 2)]
        public virtual int UserId { get; set; }
        public virtual bool IsAdministrator { get; set; }
    }
}