using System;
using System.Collections.Generic;

namespace SupportManager.DAL;

public class ScheduleTemplate : Entity
{
    public virtual SupportTeam Team { get; set; }
    public virtual int TeamId { get; set; }

    public virtual string Name { get; set; }

    public virtual DayOfWeek StartDay { get; set; }

    public virtual ICollection<ScheduleTemplateEntry> Entries { get; set; }
}