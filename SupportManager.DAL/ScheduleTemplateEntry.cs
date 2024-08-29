using System;

namespace SupportManager.DAL;

public class ScheduleTemplateEntry : Entity
{
    public virtual ScheduleTemplate Template { get; set; }
    public virtual int TemplateId { get; set; }

    public virtual DayOfWeek DayOfWeek  { get; set; }
    public virtual TimeSpan Time { get; set; }
    public virtual int UserSlot { get; set; }
}