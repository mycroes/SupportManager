using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreLinq;
using NodaTime;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams.Pages.Report
{
    public class MonthlyModel : PageModel
    {
        private readonly IMediator mediator;

        public MonthlyModel(IMediator mediator) => this.mediator = mediator;

        public Result Data { get; set; }

        public async Task OnGetAsync(Query query)
        {
            Data = await mediator.Send(query.Year == 0
                ? query with { Year = DateTime.Now.Year, Month = DateTime.Now.Month }
                : query);
        }

        public record Query(int TeamId, int Year, int Month) : IRequest<Result>;

        public class Ref
        {
            public LocalDate Date { get; }
            public Query Query { get; }

            public Ref(int teamId, LocalDate date)
            {
                Date = date;
                Query = new Query(teamId, date.Year, date.Month);
            }
        }

        public class Result
        {
            public Result(int teamId, int year, int month)
            {
                TeamId = teamId;
                Year = year;
                Month = month;

                Date = new LocalDate(Year, Month, 1);
                Previous = new Ref(teamId, Date.PlusMonths(-1));
                Next = new Ref(teamId, Date.PlusMonths(1));
            }

            public LocalDate Date { get; }
            public int Year { get; }
            public int Month { get; }
            public int TeamId { get; }

            public List<Week> Weeks { get; set; }
            public Ref Previous { get; }
            public Ref Next { get; }

            public class Week
            {
                public LocalDate Start { get; set; }
                public LocalDate End { get; set; }
                public List<TimeSlot> Slots { get; set; }
                public List<Summary> Summaries { get; set; }
            }

            public class TimeSlot
            {
                public DateTimeOffset StartTime { get; set; }
                public DateTimeOffset EndTime { get; set; }
                public string GroupingKey { get; set; }
                public List<Participation> Participations { get; set; }
            }

            public class Summary
            {
                public TimeSpan Duration { get; set; }
                public string GroupingKey { get; set; }
                public List<Participation> Participations { get; set; }
            }

            public class Participation
            {
                public string UserName { get; set; }
                public TimeSpan Duration { get; set; }
            }
        }

        public class TimeSlot
        {
            public TimeSlot(TimeSpan start, string groupingKey)
            {
                Start = start;
                GroupingKey = groupingKey;
            }

            public TimeSpan Start { get; set; }
            public string GroupingKey { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                TimeSlot BuildSlot(DayOfWeek day, double hours, string groupingKey)
                {
                    return new TimeSlot(TimeSpan.FromDays((int) day).Add(TimeSpan.FromHours(hours)), groupingKey);
                }

                const string WORK = "Kantooruren";
                const string WEEK = "Doordeweeks";
                const string WEEKEND = "Weekend";

                var weekSlots = new List<TimeSlot>();
                for (var day = DayOfWeek.Monday; day < DayOfWeek.Friday; day++)
                {
                    weekSlots.Add(BuildSlot(day, 7.5, WORK));
                    weekSlots.Add(BuildSlot(day, 16.5, WEEK));
                }

                weekSlots.Add(BuildSlot(DayOfWeek.Friday, 7.5, WORK));
                weekSlots.Add(BuildSlot(DayOfWeek.Friday, 16.5, WEEKEND));

                var dt = new DateTime(request.Year, request.Month, 1);
                int dayOfWeek = (int) dt.DayOfWeek;
                var resultStart = dt.AddDays(-dayOfWeek).Add(weekSlots[0].Start);
                var nextMonth = dt.AddMonths(1);
                var resultEnd = nextMonth.AddDays(7 - (int) nextMonth.DayOfWeek).Add(weekSlots[0].Start);
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
                var slots = new List<(Result.Week, DateTime, string)>();
                for (var weekStart = resultStart; weekStart < resultEnd; weekStart = weekStart.AddDays(7))
                {
                    var start = LocalDate.FromDateTime(weekStart);
                    var week = new Result.Week
                    {
                        Start = start,
                        End = start.PlusDays(6),
                        Slots = new List<Result.TimeSlot>(), Summaries = new List<Result.Summary>(),
                    };
                    weeks.Add(week);
                    slots.AddRange(weekSlots.Select(s => (week, weekStart.Add(s.Start).Subtract(weekSlots[0].Start), s.GroupingKey)));
                }

                slots.Add((null, resultEnd, null)); // Add end

                var registrations = db.ForwardingStates.AsNoTracking().Where(s => s.TeamId == request.TeamId);
                var lastBefore = await registrations.Where(s => s.When < resultStart)
                    .OrderByDescending(s => s.When)
                    .FirstOrDefaultAsync();

                var inRange = await registrations.Where(s => s.When >= resultStart && s.When <= resultEnd)
                    .OrderBy(s => s.When)
                    .ToListAsync();

                if (lastBefore != null) inRange.Insert(0, lastBefore);

                if (!inRange.Any())
                    return new Result(request.TeamId, request.Year, request.Month) {Weeks = new List<Result.Week>()};

                foreach (var (week, start, end, groupingKey) in GetSlots(slots))
                {
                    var before = inRange.TakeWhile(res => res.When < start).ToList();
                    var skip = before.Count - 1;
                    if (skip == -1) skip = 0;
                    var thisSlot = inRange.Skip(skip).TakeUntil(res => res.When > end).ToList();
                    var results = new List<(string user, TimeSpan duration)>();

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

                        if (thisSlot[j].DetectedPhoneNumber != null) results.Add((thisSlot[j].DetectedPhoneNumber.User.DisplayName, pEnd - pStart));
                    }

                    var last = thisSlot[thisSlot.Count - 1];
                    if (last.When < end)
                    {

                        var pStart = last.When;
                        if (pStart < start) pStart = start;

                        if (last.DetectedPhoneNumber != null) results.Add((last.DetectedPhoneNumber.User.DisplayName, end - pStart));
                    }

                    week.Slots.Add(new Result.TimeSlot
                    {
                        StartTime = start,
                        EndTime = end,
                        GroupingKey = groupingKey,
                        Participations = results.GroupBy(r => r.user)
                            .Select(g => new Result.Participation
                            {
                                Duration = TimeSpan.FromSeconds(g.Sum(x => x.duration.TotalSeconds)),
                                UserName = g.Key
                            })
                            .OrderByDescending(p => p.Duration)
                            .ToList()
                    });
                }

                foreach (var week in weeks)
                {
                    var grouped = week.Slots.GroupBy(s => s.GroupingKey);
                    foreach (var group in grouped)
                    {
                        Dictionary<string, TimeSpan> participations = new Dictionary<string, TimeSpan>();
                        foreach (var p in group.SelectMany(g => g.Participations))
                        {
                            if (participations.TryGetValue(p.UserName, out var duration))
                            {
                                duration += p.Duration;
                            }
                            else
                            {
                                duration = p.Duration;
                            }

                            participations[p.UserName] = duration;
                        }

                        var summary = new Result.Summary
                        {
                            Duration = TimeSpan.FromSeconds(group.Sum(g => (g.EndTime - g.StartTime).TotalSeconds)),
                            GroupingKey = group.Key,
                            Participations =
                                participations.Select(x =>
                                    new Result.Participation {Duration = x.Value, UserName = x.Key}).ToList()
                        };

                        week.Summaries.Add(summary);
                    }
                }

                return new Result(request.TeamId, request.Year, request.Month) {Weeks = weeks};
            }

            private IEnumerable<(Result.Week, DateTime, DateTime, string)> GetSlots(List<(Result.Week week, DateTime start, string groupingKey)> startTimes)
            {
                for (int i = 0; i < startTimes.Count - 1; i++)
                {
                    yield return (startTimes[i].week, startTimes[i].start, startTimes[i + 1].start, startTimes[i].groupingKey);
                }
            }
        }
    }
}
