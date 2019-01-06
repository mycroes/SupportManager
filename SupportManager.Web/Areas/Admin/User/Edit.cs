using System;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Areas.Admin.User
{
    public static class Edit
    {
        public class Command : IRequest
        {
            public int Id { get; set; }
            public string DisplayName { get; set; }
            public string Login { get; set; }
            public string PrimaryEmailAddress { get; set; }
            public string PrimaryPhoneNumber { get; set; }
        }

        public class Query : IRequest<Command>
        {
            public int Id { get; set; }
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
                var user = await db.Users.FindAsync(message.Id) ??
                    throw new ArgumentException($"Couldn't find User with Id '{message.Id}'.");

                user.DisplayName = message.DisplayName;
                user.Login = message.Login;

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

                await db.SaveChangesAsync();
            }
        }
    }
}
