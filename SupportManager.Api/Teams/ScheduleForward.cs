using System;

namespace SupportManager.Api.Teams
{
    public class ScheduleForward
    {
        public int TeamId { get; set; }
        public int PhoneNumberId { get; set; }
        public DateTimeOffset When { get; set; }
    }
}