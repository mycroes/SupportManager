using System.Data.Entity;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Admin.Pages.User
{
    public class EditModel : PageModel
    {
        private readonly IMediator mediator;

        public EditModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public async Task OnGetAsync(Query query) => Data = await mediator.Send(query);

        public async Task<IActionResult> OnPostAsync()
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index));
        }
        public record Query(int Id) : IRequest<Command>;

        public class Command : IRequest
        {
            public int Id { get; init; }
            public string DisplayName { get; init; }
            public string Login { get; init; }
            public string PrimaryEmailAddress { get; init; }
            public string PrimaryPhoneNumber { get; init; }
            public bool Deleted { get; set; }
            public bool IsSuperUser { get; set; }
            public SupportTeam Team { get; set; }
            public bool IsTeamAdmin { get; set; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.DisplayName).NotNull();
                RuleFor(m => m.Login).NotNull();
            }
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
                    PrimaryPhoneNumber = u.PrimaryPhoneNumber.Value,
                    Deleted = u.Deleted,
                    IsSuperUser = u.IsSuperUser,
                    Team = u.Memberships.FirstOrDefault().Team,
                    IsTeamAdmin = u.Memberships.Any() && u.Memberships.FirstOrDefault().IsAdministrator
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
                var user = await db.Users.FindAsync(message.Id) ??
                    throw new ArgumentException($"Couldn't find User with Id '{message.Id}'.");
                await db.Entry(user).Collection(u => u.Memberships).LoadAsync();

                user.DisplayName = message.DisplayName;
                user.Login = message.Login;
                user.IsSuperUser = message.IsSuperUser;
                user.Deleted = message.Deleted;

                if (message.PrimaryEmailAddress == null)
                {
                    if (user.PrimaryEmailAddress != null)
                    {
                        db.UserEmailAddresses.Remove(user.PrimaryEmailAddress);
                        user.PrimaryEmailAddress = null;
                    }
                }
                else
                {
                    if (user.PrimaryEmailAddress == null)
                    {
                        user.PrimaryEmailAddress = new UserEmailAddress
                        {
                            User = user, Value = message.PrimaryEmailAddress, IsVerified = true
                        };
                    }
                    else
                    {
                        user.PrimaryEmailAddress.Value = message.PrimaryEmailAddress;
                    }
                }

                if (message.PrimaryPhoneNumber == null)
                {
                    if (user.PrimaryPhoneNumber != null)
                    {
                        db.UserPhoneNumbers.Remove(user.PrimaryPhoneNumber);
                        user.PrimaryPhoneNumber = null;
                    }
                }
                else
                {
                    if (user.PrimaryPhoneNumber == null)
                    {
                        user.PrimaryPhoneNumber = new UserPhoneNumber
                        {
                            User = user, Label = "Primary", Value = message.PrimaryPhoneNumber, IsVerified = true
                        };
                    }
                    else
                    {
                        user.PrimaryPhoneNumber.Value = message.PrimaryPhoneNumber;
                    }
                }

                if (message.Team == null)
                {
                    user.Memberships.Clear();
                }
                else
                {
                    if (!user.Memberships.Any() || user.Memberships.Any(x => x.TeamId != message.Team.Id))
                    {
                        user.Memberships.Clear();
                        user.Memberships.Add(new TeamMember
                        {
                            Team = message.Team
                        });
                    }

                    user.Memberships.First().IsAdministrator = message.IsTeamAdmin;
                }

                await db.SaveChangesAsync();
            }
        }
    }
}
