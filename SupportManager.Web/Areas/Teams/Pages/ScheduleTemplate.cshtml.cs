using System.Data.Entity;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.Contracts;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;
using SupportManager.Web.Infrastructure.CRUD;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class ScheduleTemplateModel(IMediator mediator) : TeamPageModel
    {
        [BindProperty]
        public Command Data { get; set; }

        public record Command(ScheduleTemplate ScheduleTemplate, DateOnly? StartDate, List<Slot> Slots) : IRequest
        {
            public class Validator : AbstractValidator<Command>
            {
                public Validator()
                {
                    RuleFor(x => x.ScheduleTemplate).NotNull();
                    RuleFor(x => x.StartDate).NotNull();
                    RuleFor(x => x.Slots).NotNull().NotEmpty();
                }
            }
        }

        public record Slot(int UserSlot, UserPhoneNumber PhoneNumber)
        {
            public class Validator : AbstractValidator<Slot>
            {
                public Validator()
                {
                    RuleFor(x => x.PhoneNumber).NotNull();
                }
            }
        }

        public async Task OnGetAsync(Query<Command> query)
        {
            Data = await mediator.Send(query);
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index), new { TeamId });
        }

        private static Exception NotFound(int teamId, int id, string parameterName) =>
            new ArgumentException($"Schedule template for team {teamId} with Id {id} could not be found.",
                parameterName);

        public class QueryHandler(SupportManagerContext db, TeamId teamId) : IRequestHandler<Query<Command>, Command>
        {
            public async Task<Command> Handle(Query<Command> request, CancellationToken cancellationToken)
            {
                var template =
                    await db.ScheduleTemplates.Where(t => t.TeamId == teamId.Value).Where(t => t.Id == request.Id)
                        .Include(t => t.Entries).FirstOrDefaultAsync(cancellationToken) ??
                    throw NotFound(teamId.Value, request.Id, nameof(request));

                return new Command(template, null,
                    template.Entries.DistinctBy(e => e.UserSlot).OrderBy(e => e.UserSlot)
                        .Select(e => new Slot(e.UserSlot, null)).ToList());
            }
        }

        public class CommandHandler(SupportManagerContext db, TeamId teamId) : AsyncRequestHandler<Command>
        {
            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var template = request.ScheduleTemplate;
                await db.Entry(template).Collection(t => t.Entries).LoadAsync(cancellationToken);

                var slots = request.Slots.ToDictionary(s => s.UserSlot, s => s.PhoneNumber);

                var scheduledForwards = template.Entries.Select(e => new ScheduledForward
                {
                    TeamId = teamId.Value,
                    PhoneNumber = slots[e.UserSlot],
                    When = GetScheduledDateTime(template, e, request.StartDate!.Value)
                }).ToList();

                db.ScheduledForwards.AddRange(scheduledForwards);
                db.BeginTransaction();
                await db.SaveChangesAsync(cancellationToken);

                scheduledForwards.ForEach(scheduledForward => scheduledForward.ScheduleId =
                    BackgroundJob.Schedule<IForwarder>(f => f.ApplyScheduledForward(scheduledForward.Id, null),
                        scheduledForward.When));

                await db.SaveChangesAsync(cancellationToken);
                await db.CommitTransactionAsync();
            }

            private static DateTimeOffset GetScheduledDateTime(ScheduleTemplate template, ScheduleTemplateEntry entry,
                DateOnly startDate)
            {
                var dayDiff = entry.DayOfWeek - template.StartDay;
                if (dayDiff < 0 || dayDiff == 0 && entry.Time < template.StartTime) dayDiff += 7;

                return startDate.AddDays(dayDiff).ToDateTime(TimeOnly.FromTimeSpan(entry.Time));
            }
        }
    }
}
