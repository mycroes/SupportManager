using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Areas.Teams.Pages.Shared;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Pages.Admin.Members
{
    public class AddModel : TeamPageModel
    {
        private readonly IMediator mediator;

        public AddModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public void OnGet() => Data = new Command { TeamId = TeamId };

        public async Task<IActionResult> OnPostAsync()
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index), new { TeamId });
        }

        public record Command : IRequest
        {
            public int TeamId { get; set; }
            public string DisplayName { get; init; }
            public string Login { get; init; }
            public string PrimaryEmailAddress { get; init; }
            public string PrimaryPhoneNumber { get; init; }
        }

        public class Validator : AbstractValidator<Command>
        {
            public Validator()
            {
                RuleFor(m => m.DisplayName).NotNull();
                RuleFor(m => m.Login).NotNull();
            }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command message, CancellationToken cancellationToken)
            {
                var user = new User { DisplayName = message.DisplayName, Login = message.Login };
                db.Users.Add(user);
                db.TeamMembers.Add(new TeamMember { User = user, TeamId = message.TeamId });

                await db.SaveChangesAsync();

                if (message.PrimaryEmailAddress != null)
                {
                    user.PrimaryEmailAddress = new UserEmailAddress
                    {
                        User = user, Value = message.PrimaryEmailAddress, IsVerified = true
                    };
                }

                if (message.PrimaryPhoneNumber != null)
                {
                    user.PrimaryPhoneNumber = new UserPhoneNumber
                    {
                        User = user, Label = "Primary", Value = message.PrimaryPhoneNumber, IsVerified = true
                    };
                }
            }
        }
    }
}
