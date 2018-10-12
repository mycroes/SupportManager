using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Pages.User.ApiKeys
{
    public class CreateModel : PageModel
    {
        private readonly IMediator mediator;

        public CreateModel(IMediator mediator) => this.mediator = mediator;

        [BindProperty(SupportsGet = true)]
        public Command Data { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await mediator.Send(Data);
            return RedirectToPage("List");
        }

        public class Command : IRequest, IBindLoggedInUser
        {
            public DAL.User LoggedInUser { get; set; }
        }

        public class Handler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db) => this.db = db;

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var value = Base64UrlTextEncoder.Encode(Guid.NewGuid().ToByteArray());
                db.ApiKeys.Add(new ApiKey {User = request.LoggedInUser, Value = value});

                await db.SaveChangesAsync();
            }
        }
    }
}