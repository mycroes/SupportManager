using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Refit;
using SupportManager.Api;
using SupportManager.Api.Events;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.DAL;

namespace SupportManager.Web.Pages.User.ApiKeys
{
    public class TestModel : PageModel
    {
        private readonly IMediator mediator;

        public TestModel(IMediator mediator) => this.mediator = mediator;

        public async Task OnGetAsync(int id)
        {
            await mediator.Send(new Command(id));
        }

        public class Command : IRequest
        {
            public Command(int id)
            {
                Id = id;
            }

            public int Id { get; }
        }

        public class CommandHandler : AsyncRequestHandler<Command>
        {
            private readonly SupportManagerContext db;

            public CommandHandler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task Handle(Command request, CancellationToken cancellationToken)
            {
                var apiKey = await db.ApiKeys.FindAsync(request.Id);
                var callback = RestService.For<ICallback>(apiKey.CallbackUrl);
                await callback.ForwardChanged(new ForwardChanged
                {
                    OldForward = new ForwardRegistration
                    {
                        PhoneNumber =
                            new PhoneNumber
                            {
                                Id = apiKey.UserId, Label = "Previous number", Value = "0123456789"
                            },
                        User = new SupportManager.Api.Users.User
                        {
                            DisplayName = "Previous name", Id = apiKey.UserId
                        },
                        When = DateTimeOffset.Now - TimeSpan.FromMinutes(10)
                    },
                    NewForward = new ForwardRegistration
                    {
                        PhoneNumber =
                            new PhoneNumber {Id = apiKey.UserId, Label = "Next number", Value = "0123456789"},
                        User = new SupportManager.Api.Users.User
                        {
                            DisplayName = "Next name", Id = apiKey.UserId
                        },
                        When = DateTimeOffset.Now
                    },
                    Team = new Team {Id = -1, Name = "Test"}
                });
            }
        }
    }
}
