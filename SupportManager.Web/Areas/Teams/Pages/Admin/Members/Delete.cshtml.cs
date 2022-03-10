using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.Members
{
    public class DeleteModel : TeamPageModel
    {
        private readonly IMediator mediator;

        public DeleteModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query) => Data = await mediator.Send(query);

        public async Task<IActionResult> OnPostAsync()
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index), new { TeamId });
        }
        public record Query(int Id) : IRequest<Command>;

        public class Command : IRequest
        {
            public int Id { get; init; }
            public int TeamId { get; init; }
            public string DisplayName { get; init; }
            public string Login { get; init; }
            public string PrimaryEmailAddress { get; init; }
            public string PrimaryPhoneNumber { get; init; }
        }

        public class QueryHandler : IRequestHandler<Query, Command>
        {
            private readonly SupportManagerContext db;

            public QueryHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Command> Handle(Query request, CancellationToken cancellationToken)
            {
                return await db.Users.Where(u => u.Id == request.Id).Select(u => new Command
                {
                    Id = u.Id,
                    DisplayName = u.DisplayName,
                    Login = u.Login,
                    PrimaryEmailAddress = u.PrimaryEmailAddress.Value,
                    PrimaryPhoneNumber = u.PrimaryPhoneNumber.Value
                }).SingleOrDefaultAsync() ?? throw new Exception($"Couldn't find User with Id '{request.Id}.");
            }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command message, CancellationToken cancellationToken)
            {
                var user = await db.Users.Include(u => u.Memberships).SingleOrDefaultAsync(u => u.Id == message.Id) ??
                    throw new ArgumentException($"Couldn't find User with Id '{message.Id}'.");

                if (!user.DisplayName.EndsWith("(Deleted)")) user.DisplayName += " (Deleted)";

                user.Deleted = true;
                db.TeamMembers.RemoveRange(user.Memberships);

                var pendingForwards = await db.ScheduledForwards.Where(fwd => fwd.PhoneNumber.UserId == user.Id)
                    .Where(fwd => fwd.When > DateTimeOffset.Now).Where(fwd => !fwd.Deleted).ToListAsync();
                pendingForwards.ForEach(fwd => fwd.Deleted = true);

                await db.SaveChangesAsync();
            }
        }
    }
}
