using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Admin.Pages.User
{
    public class CreateModel : PageModel
    {
        private readonly IMediator mediator;

        public CreateModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty]
        public Command Data { get; set; }

        public void OnGet() => Data = new Command();

        public async Task<IActionResult> OnPostAsync()
        {
            await mediator.Send(Data);

            return this.RedirectToPageJson(nameof(Index));
        }

        public record Command : IRequest
        {
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
                var user = new DAL.User { DisplayName = message.DisplayName, Login = message.Login };
                db.Users.Add(user);
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
