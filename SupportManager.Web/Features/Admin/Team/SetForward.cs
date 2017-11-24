using Hangfire;
using MediatR;
using SupportManager.Contracts;

namespace SupportManager.Web.Features.Admin.Team
{
    public static class SetForward
    {
        public class Command : IRequest
        {
            public int TeamId { get; set; }
            public int PhoneNumberId { get; set; }
        }

        public class Handler : IRequestHandler<Command>
        {
            public void Handle(Command message)
            {
                BackgroundJob.Enqueue<IForwarder>(f => f.ApplyForward(message.TeamId, message.PhoneNumberId, null));
            }
        }
    }
}