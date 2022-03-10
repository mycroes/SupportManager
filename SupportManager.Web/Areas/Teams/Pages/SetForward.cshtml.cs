using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Contracts;
using SupportManager.Web.Areas.Teams.Pages.Shared;

namespace SupportManager.Web.Areas.Teams.Pages
{
    public class SetForwardModel : TeamPageModel
    {
        private readonly IMediator mediator;

        public SetForwardModel(IMediator mediator) => this.mediator = mediator;

        public async Task<IActionResult> OnGetAsync(Command command)
        {
            await mediator.Send(command);

            return RedirectToPage(nameof(Index), new { TeamId });
        }

        public class Command : IRequest
        {
            public int TeamId { get; set; }
            public int PhoneNumberId { get; set; }
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
