using System;

namespace SupportManager.DAL
{
    public class ScheduledForward : Entity
    {
        public virtual SupportTeam Team { get; set; }
        public virtual int TeamId { get; set; }
        public virtual DateTimeOffset When { get; set; }
        public virtual UserPhoneNumber PhoneNumber { get; set; }
        public virtual int PhoneNumberId { get; set; }
        public virtual string ScheduleId { get; set; }
    }
}