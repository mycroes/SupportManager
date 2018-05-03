using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.Admin.User
{
    public class Create
    {
        public class Command : IRequest
        {
            public string DisplayName { get; set; }
            public string Login { get; set; }
            public string PrimaryEmailAddress { get; set; }
            public string PrimaryPhoneNumber { get; set; }
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

            protected override async Task HandleCore(Command message)
            {
                var user = new DAL.User {DisplayName = message.DisplayName, Login = message.Login};
                db.Users.Add(user);
                await db.SaveChangesAsync();

                if (message.PrimaryEmailAddress != null)
                {
                    user.PrimaryEmailAddress =
                        new UserEmailAddress {User = user, Value = message.PrimaryEmailAddress, IsVerified = true};
                }

                if (message.PrimaryPhoneNumber != null)
                {
                    user.PrimaryPhoneNumber = new UserPhoneNumber
                    {
                        User = user,
                        Label = "Primary",
                        Value = message.PrimaryPhoneNumber,
                        IsVerified = true
                    };
                }
            }
        }
    }
}