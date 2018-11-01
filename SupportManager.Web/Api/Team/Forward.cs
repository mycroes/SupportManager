using Hangfire;
using MediatR;
using SupportManager.Contracts;

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