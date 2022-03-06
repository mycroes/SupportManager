using System.Data.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SupportManager.DAL;

namespace SupportManager.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IMediator mediator;

        public IndexModel(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var result = await mediator.Send(new Query(User.Identity?.Name));

            return RedirectToPage(result.IsExistingUser ? "User/Index" : "User/Welcome");
        }

        public record Query(string UserName) : IRequest<Result>;

        public record Result
        {
            public string UserName { get; init; }
            public bool IsExistingUser { get; init; }
            public DAL.User User { get; init; }
        }

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
            {
                var user = await db.Users.Where(u => u.Login == request.UserName).SingleOrDefaultAsync();

                return new Result { UserName = request.UserName, IsExistingUser = user != null, User = user };
            }
        }
    }
}
