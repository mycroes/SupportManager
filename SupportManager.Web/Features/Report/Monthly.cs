using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using MoreLinq;
using SupportManager.DAL;

namespace SupportManager.Web.Features.Report
{
    public static class Monthly
    {
        public class Query : IRequest<Result>
        {
            public int TeamId { get; set; }
            public int Year { get; set; }
            public int Month { get; set; }
        }

        public class Result
        {
            public List<Week> Weeks { get; set; }

            public class Week
            {
                public List<TimeSlot> Slots { get; set; }
            }

            public class TimeSlot
            {
                public DateTimeOffset StartTime { get; set; }
                public DateTimeOffset EndTime { get; set; }
                public List<Participation> Participations { get; set; }
            }

            public class Participation
            {
                public string UserName { get; set; }
                public TimeSpan Duration { get; set; }
            }
        }

        public class Handler : IAsyncRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Query message)
            {
                var weekSlots = new List<TimeSpan>
                {
                    TimeSpan.FromDays((int) DayOfWeek.Monday).Add(TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(30))),
                    TimeSpan.FromDays((int) DayOfWeek.Friday).Add(TimeSpan.FromHours(16).Add(TimeSpan.FromMinutes(30)))
                };

                var dt = new DateTime(message.Year, message.Month, 1);
                int dayOfWeek = (int) dt.DayOfWeek;
                var resultStart = dt.AddDays(-dayOfWeek).Add(weekSlots[0]);
                var nextMonth = dt.AddMonths(1);
                var resultEnd = nextMonth.AddDays(7 - (int) nextMonth.DayOfWeek).Add(weekSlots[0]);
                if (resultStart.Month == dt.Month && resultStart.Day > 1)
                {
                    resultStart = resultStart.AddDays(-7);
                }

                if (resultEnd.Day > 6)
                {
                    resultEnd = resultEnd.AddDays(-7);
                }

                if (resultEnd > DateTime.Now) resultEnd = DateTime.Now;

                var weeks = new List<Result.Week>();
                var slots = new List<(Result.Week, DateTime)>();
                for (var weekStart = resultStart; weekStart < resultEnd; weekStart = weekStart.AddDays(7))
                {
                    var week = new Result.Week {Slots = new List<Result.TimeSlot>()};
                    weeks.Add(week);
                    slots.AddRange(weekSlots.Select(s => (week, weekStart.Add(s).Subtract(weekSlots[0]))));
                }

                slots.Add((null, resultEnd)); // Add end

                var registrations = db.ForwardingStates.AsNoTracking().Where(s => s.TeamId == message.TeamId);
                var lastBefore = await registrations.Where(s => s.When < resultStart)
                    .OrderByDescending(s => s.When)
                    .FirstOrDefaultAsync();

                var inRange = await registrations.Where(s => s.When >= resultStart && s.When <= resultEnd)
                    .OrderBy(s => s.When)
                    .ToListAsync();

                if (lastBefore != null) inRange.Insert(0, lastBefore);

                if (!inRange.Any()) return new Result {Weeks = new List<Result.Week>()};

                foreach (var (week, start, end) in GetSlots(slots))
                {
                    var before = inRange.TakeWhile(res => res.When < start).ToList();
                    var skip = before.Count - 1;
                    if (skip == -1) skip = 0;
                    var thisSlot = inRange.Skip(skip).TakeUntil(res => res.When > end).ToList();
                    var results = new List<(string, TimeSpan)>();

                    if (!thisSlot.Any() || thisSlot[0].When > end)
                    {
                        results.Add((null, end.Subtract(start)));
                        continue;
                    }
                    if (thisSlot[0].When > start) results.Add((null, thisSlot[0].When.Subtract(start)));
                    for (int j = 0; j < thisSlot.Count - 1; j++)
                    {
                        var pStart = thisSlot[j].When;
                        if (pStart < start) pStart = start;

                        var pEnd = thisSlot[j + 1].When;
                        if (pEnd > end) pEnd = end;

                        results.Add((thisSlot[j].DetectedPhoneNumber.User.DisplayName, pEnd - pStart));
                    }

                    var last = thisSlot[thisSlot.Count - 1];
                    if (last.When < end)
                    {
                        
                        var pStart = last.When;
                        if (pStart < start) pStart = start;

                        results.Add((last.DetectedPhoneNumber.User.DisplayName, end - pStart));
                    }

                    week.Slots.Add(new Result.TimeSlot
                    {
                        StartTime = start,
                        EndTime = end,
                        Participations = results.GroupBy(r => r.Item1)
                            .Select(g => new Result.Participation
                            {
                                Duration = TimeSpan.FromSeconds(g.Sum(x => x.Item2.TotalSeconds)),
                                UserName = g.Key
                            })
                            .OrderByDescending(p => p.Duration)
                            .ToList()
                    });
                }

                return new Result {Weeks = weeks};
            }

            private IEnumerable<(Result.Week, DateTime, DateTime)> GetSlots(List<(Result.Week, DateTime)> startTimes)
            {
                for (int i = 0; i < startTimes.Count - 1; i++)
                {
                    yield return (startTimes[i].Item1, startTimes[i].Item2, startTimes[i + 1].Item2);
                }
            }
        }
    }
}
