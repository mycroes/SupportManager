using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreLinq;
using NodaTime;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Teams.Pages.Report
{
    public class DailyModel : PageModel
    {
        private readonly IMediator mediator;

        public DailyModel(IMediator mediator) => this.mediator = mediator;
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
                public List<Day> Days { get; set; }
            }

            public class Day
            {
                public LocalDate Date { get; set; }
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

                // First start moment within this group (slot/week/day)
                public DateTimeOffset? FirstStart { get; set; }
            }
        }

        public class TimeSlot
        {
            public TimeSpan Start { get; set; }
            public string GroupingKey { get; set; }
            public TimeSlot(TimeSpan start, string groupingKey)
            {
                Start = start;
                this.GroupingKey = groupingKey;
            }
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
                List<TimeSlot> weekSlots = CreateWeekSlots();

                var dt = new DateTime(request.Year, request.Month, 1);
                var resultStart = GetResultStart(weekSlots, dt);
                var resultEnd = GetResultEnd(weekSlots, dt);

                var forwardingStates = await GetForwardingStatesInRange(request, resultStart, resultEnd);

                if (!forwardingStates.Any()) return new Result(request.TeamId, request.Year, request.Month) { Weeks = new List<Result.Week>()};

                // clamp resultEnd to the last real record in this period
                var lastRealState = forwardingStates
                    .Where(s => s.When >= resultStart)
                    .OrderByDescending(s => s.When)
                    .FirstOrDefault();

                if (lastRealState != null && lastRealState.When < resultEnd)
                {
                    resultEnd = lastRealState.When.DateTime; 
                }

                List<Result.Week> weeks = GetWeeks(weekSlots, resultStart, resultEnd, forwardingStates, lastRealState);

                // Week-samenvattingen
                foreach (var week in weeks) week.Summaries.AddRange(GetWeekSummaries(week));

                // Dag-samenvattingen (per dag per categorie)
                foreach (var week in weeks)
                {
                    foreach (var day in week.Days)
                    {
                        day.Summaries.AddRange(GetDaySummaries(day));

                    }
                }

                return new Result(request.TeamId, request.Year, request.Month)
                {
                    Weeks = weeks
                };
            }

            internal static List<TimeSlot> CreateWeekSlots()
            {
                int Normalize(DayOfWeek day) => day == DayOfWeek.Sunday ? 7 : (int)day;
                
                TimeSlot BuildSlot(DayOfWeek day, double hours, string groupingKey)
                {
                    return new TimeSlot(TimeSpan.FromDays(Normalize(day)).Add(TimeSpan.FromHours(hours)), groupingKey);
                }

                const string WORK = "Kantooruren";
                const string WEEK = "Doordeweeks";
                const string WEEKEND = "Weekend";

                var weekSlots = new List<TimeSlot>();

                // Monday through Thursday
                for (var day = DayOfWeek.Monday; day < DayOfWeek.Friday; day++)
                {
                    weekSlots.Add(BuildSlot(day, 7.5, WORK));   // 07:30
                    weekSlots.Add(BuildSlot(day, 16.5, WEEK));  // 16:30
                }

                // Friday office hours
                weekSlots.Add(BuildSlot(DayOfWeek.Friday, 7.5, WORK));

                // --- WEEKEND ---
                weekSlots.Add(BuildSlot(DayOfWeek.Friday, 16.5, WEEKEND)); // start weekend shift 16:30
                weekSlots.Add(BuildSlot(DayOfWeek.Saturday, 7.5, WEEKEND)); // Saturday 07:30
                weekSlots.Add(BuildSlot(DayOfWeek.Sunday, 7.5, WEEKEND));   // Sunday 07:30

                return weekSlots;
            }

            internal static DateTime GetResultStart(List<TimeSlot> weekSlots, DateTime dt)
            {
                var dayOfWeek = (int)dt.DayOfWeek;
                var resultStart = dt.AddDays(-dayOfWeek).Add(weekSlots[0].Start);
                if (resultStart.Month == dt.Month && resultStart.Day > 1) resultStart = resultStart.AddDays(-7);
                return resultStart;
            }

            internal static DateTime GetResultEnd(List<TimeSlot> weekSlots, DateTime dt)
            {
                var nextMonth = dt.AddMonths(1);
                var resultEnd = nextMonth.AddDays(7 - (int)nextMonth.DayOfWeek).Add(weekSlots[0].Start);
                if (resultEnd.Day > 6) resultEnd = resultEnd.AddDays(-7);
                if (resultEnd > DateTime.Now) resultEnd = DateTime.Now;
                return resultEnd;
            }

            internal async Task<List<ForwardingState>> GetForwardingStatesInRange(Query request, DateTime resultStart, DateTime resultEnd)
            {
                var registrations = db.ForwardingStates.AsNoTracking().Where(s => s.TeamId == request.TeamId);
                var lastBefore = await registrations.Where(s => s.When < resultStart)
                    .OrderByDescending(s => s.When)
                    .FirstOrDefaultAsync();

                var inRange = await registrations
                    .Where(s => s.When >= resultStart && s.When <= resultEnd)
                    .OrderBy(s => s.When)
                    .ToListAsync();

                if (lastBefore != null) inRange.Insert(0, lastBefore);
                return inRange;
            }

            internal static List<Result.Week> GetWeeks(List<TimeSlot> weekSlots, DateTime resultStart, DateTime resultEnd, List<ForwardingState> forwardingStates, ForwardingState lastRealState)
            {
                var weeks = new List<Result.Week>();
                var slots = new List<(Result.Week week, DateTime start, string groupingKey)>();

                for (var weekStart = resultStart; weekStart < resultEnd; weekStart = weekStart.AddDays(7))
                {
                    var start = LocalDate.FromDateTime(weekStart);
                    var week = new Result.Week
                    {
                        Start = start,
                        End = start.PlusDays(6),
                        Slots = new List<Result.TimeSlot>(),
                        Summaries = new List<Result.Summary>(),
                        Days = new List<Result.Day>()
                    };

                    // 7 days within the week
                    for (var i = 0; i < 7; i++)
                    {
                        week.Days.Add(new Result.Day
                        {
                            Date = start.PlusDays(i),
                            Slots = new List<Result.TimeSlot>(),
                            Summaries = new List<Result.Summary>()
                        });
                    }

                    weeks.Add(week);

                    slots.AddRange(weekSlots.Select(s =>
                        (week,
                         weekStart.Add(s.Start).Subtract(weekSlots[0].Start),
                         s.GroupingKey)));
                }

                slots.Add((null, resultEnd, null));

                foreach (var (week, start, end, groupingKey) in GetSlotsWithEndTime(slots))
                {
                    var before = forwardingStates.TakeWhile(res => res.When < start).ToList();
                    var skip = before.Count - 1;
                    if (skip == -1) skip = 0;

                    var thisSlot = forwardingStates
                        .Skip(skip)
                        .TakeUntil(res => res.When > end)
                        .ToList();

                    if (!thisSlot.Any() || thisSlot[0].When > end)
                    {
                        var emptySlot = new Result.TimeSlot
                        {
                            StartTime = start,
                            EndTime = end,
                            GroupingKey = groupingKey,
                            Participations = new List<Result.Participation>()
                        };

                        week.Slots.Add(emptySlot);

                        var emptyDate = LocalDate.FromDateTime(start);
                        var emptyDay = week.Days.FirstOrDefault(d => d.Date == emptyDate);
                        if (emptyDay != null)
                        {
                            emptyDay.Slots.Add(emptySlot);
                        }

                        continue;
                    }

                    // user -> (firstStart, duration)
                    var participationDict = new Dictionary<string, (DateTimeOffset firstStart, TimeSpan duration)>();

                    for (int j = 0; j < thisSlot.Count - 1; j++)
                    {
                        var state = thisSlot[j];
                        var pStart = RoundTimestampToNearestMinute(state.When);
                        if (pStart < start) pStart = start;
                        var pEnd = RoundTimestampToNearestMinute(thisSlot[j + 1].When);
                        if (pEnd > end) pEnd = end;


                        if (state.DetectedPhoneNumber == null) continue;

                        var userName = state.DetectedPhoneNumber.User.DisplayName;
                        var duration = RoundToNearestMinute(pEnd - pStart);

                        if (participationDict.TryGetValue(userName, out var info))
                        {
                            var firstStart = info.firstStart <= pStart ? info.firstStart : pStart;
                            participationDict[userName] = (firstStart, info.duration + duration);
                        }
                        else
                        {
                            participationDict[userName] = (pStart, duration);
                        }
                    }

                    var last = thisSlot[thisSlot.Count - 1];


                    // if the slot STARTS after the last real DB record, then this slot must have no participation.
                    if (start > lastRealState.When)
                    {
                        continue;   // skip adding participation
                    }
                    

                    if (last.When < end && last.DetectedPhoneNumber != null)
                    {
                        var pStart = RoundTimestampToNearestMinute(last.When);
                        if (pStart < start) pStart = start;
                        var duration = RoundToNearestMinute(end - pStart);


                        var userName = last.DetectedPhoneNumber.User.DisplayName;
                        if (participationDict.TryGetValue(userName, out var info))
                        {
                            var firstStart = info.firstStart <= pStart ? info.firstStart : pStart;
                            participationDict[userName] = (firstStart, info.duration + duration);
                        }
                        else
                        {
                            participationDict[userName] = (pStart, duration);
                        }
                    }

                    var slot = new Result.TimeSlot
                    {
                        StartTime = start,
                        EndTime = end,
                        GroupingKey = groupingKey,
                        Participations = participationDict
                            .Select(kv => new Result.Participation
                            {
                                UserName = kv.Key,
                                Duration = kv.Value.duration,
                                FirstStart = kv.Value.firstStart
                            })
                            .OrderByDescending(p => p.Duration)
                            .ToList()

                    };

                    week.Slots.Add(slot);

                    var slotDate = LocalDate.FromDateTime(start);
                    var day = week.Days.FirstOrDefault(d => d.Date == slotDate);
                    if (day != null)
                    {
                        day.Slots.Add(slot);
                    }
                }

                return weeks;
            }

            public static List<Result.Summary> GetWeekSummaries(Result.Week week)
            {
                var summaries = new List<Result.Summary>();
                var grouped = week.Slots.GroupBy(s => s.GroupingKey);
                foreach (var group in grouped)
                {
                    var participations = new Dictionary<string, (TimeSpan duration, DateTimeOffset? firstStart)>();

                    foreach (var p in group.SelectMany(g => g.Participations))
                    {
                        if (string.IsNullOrEmpty(p.UserName)) continue;

                        if (participations.TryGetValue(p.UserName, out var info))
                        {
                            var first = info.firstStart;
                            if (p.FirstStart.HasValue &&
                                (!first.HasValue || p.FirstStart.Value < first.Value))
                            {
                                first = p.FirstStart;
                            }

                            participations[p.UserName] = (info.duration + p.Duration, first);
                        }
                        else
                        {
                            participations[p.UserName] = (p.Duration, p.FirstStart);
                        }
                    }

                    summaries.Add(new Result.Summary
                    {
                        Duration = TimeSpan.FromSeconds(
                            group.Sum(g => (g.EndTime - g.StartTime).TotalSeconds)),
                        GroupingKey = group.Key,
                        Participations = participations
                            .Select(x => new Result.Participation
                            {
                                UserName = x.Key,
                                Duration = x.Value.duration,
                                FirstStart = x.Value.firstStart
                            })
                            .OrderByDescending(p => p.Duration)
                            .ToList()
                    });
                }
                return summaries;
            }

            internal static List<Result.Summary> GetDaySummaries(Result.Day day)
            {
                var summaries = new List<Result.Summary>();

                var groupedDay = day.Slots.GroupBy(s => s.GroupingKey);

                foreach (var group in groupedDay)
                {
                    var participations = new Dictionary<string, (TimeSpan duration, DateTimeOffset? firstStart)>();

                    foreach (var p in group.SelectMany(g => g.Participations))
                    {
                        if (string.IsNullOrEmpty(p.UserName)) continue;

                        if (participations.TryGetValue(p.UserName, out var info))
                        {
                            var first = info.firstStart;
                            if (p.FirstStart.HasValue &&
                                (!first.HasValue || p.FirstStart.Value < first.Value))
                            {
                                first = p.FirstStart;
                            }

                            participations[p.UserName] = (info.duration + p.Duration, first);
                        }
                        else
                        {
                            participations[p.UserName] = (p.Duration, p.FirstStart);
                        }
                    }

                    summaries.Add(new Result.Summary
                    {
                        Duration = TimeSpan.FromSeconds(
                            group.Sum(g => (g.EndTime - g.StartTime).TotalSeconds)),
                        GroupingKey = group.Key,
                        Participations = participations
                            .Select(x => new Result.Participation
                            {
                                UserName = x.Key,
                                Duration = x.Value.duration,
                                FirstStart = x.Value.firstStart
                            })
                            .OrderByDescending(p => p.Duration)
                            .ToList()
                    });
                }

                return summaries;
            }


            internal static DateTimeOffset RoundTimestampToNearestMinute(DateTimeOffset t)
            {
                if (t.Second >= 30)
                {
                    // round UP
                    return new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0, t.Offset)
                        .AddMinutes(1);
                }
                else
                {
                    // round DOWN
                    return new DateTimeOffset(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0, t.Offset);
                }
            }


            internal static TimeSpan RoundToNearestMinute(TimeSpan t)
            {
                double totalMinutes = t.TotalSeconds / 60.0;

                // Round half up (>= 30 seconds => next minute)
                int roundedMinutes = (int)Math.Round(totalMinutes, MidpointRounding.AwayFromZero);

                return TimeSpan.FromMinutes(roundedMinutes);
            }

            internal static IEnumerable<(Result.Week, DateTime, DateTime, string)> GetSlotsWithEndTime(
                List<(Result.Week week, DateTime start, string groupingKey)> startTimes)
            {
                for (int i = 0; i < startTimes.Count - 1; i++)
                {
                    yield return (startTimes[i].week,
                        startTimes[i].start,
                        startTimes[i + 1].start,
                        startTimes[i].groupingKey);
                }
            }
        }
    }
}
