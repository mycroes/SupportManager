using System.ComponentModel.DataAnnotations;

namespace SupportManager.DAL
{
    public abstract class Entity
    {
        [Key]
        public int Id { get; set; }
        public bool Deleted { get; set; }
    }
}
