using FluentValidation;
using Hangfire;
using MediatR;
using SupportManager.Contracts;
using SupportManager.DAL;
using SupportManager.Web.Api.Shared;

namespace SupportManager.Web.Api.Team
{
    public static class Forward
    {
        public class Command : IRequest
        {
            public int TeamId { get; }
            public int PhoneNumberId { get; }

            public Command(int teamId, int phoneNumberId)
            {
                TeamId = teamId;
                PhoneNumberId = phoneNumberId;
            }

            public class Validator : AbstractValidator<Command>
            {
                public Validator(SupportManagerContext db)
                {
                    RuleFor(x => x.PhoneNumberId).BelongsToTeamMember(db, x => x.TeamId);
                }
            }
        }

        public class Handler : RequestHandler<Command>
        {
            protected override void Handle(Command request)
            {
                BackgroundJob.Enqueue<IForwarder>(f => f.ApplyForward(request.TeamId, request.PhoneNumberId, null));
            }
        }
    }
}