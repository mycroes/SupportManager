using System;

namespace SupportManager.DAL
{
    public class ForwardingState : Entity
    {
        public virtual SupportTeam Team { get; set; }
        public virtual int TeamId { get; set; }
        public virtual DateTimeOffset When { get; set; }
        public virtual UserPhoneNumber DetectedPhoneNumber { get; set; }
        public virtual int? DetectedPhoneNumberId { get; set; }
        public virtual string RawPhoneNumber { get; set; }
    }
}