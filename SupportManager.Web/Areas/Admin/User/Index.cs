using System.Linq;
using System.Threading.Tasks;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Areas.Admin.User
{
    public class Index
    {
        public class Query : IRequest<Result>
        {
            public int? PageNumber { get; set; }
        }

        public class Result
        {
            public IPagedList<User> Users { get; set; }

            public class User
            {
                public int Id { get; set; }
                public string DisplayName { get; set; }
                public string Login { get; set; }
                public string PrimaryPhoneNumberValue { get; set; }
            }
        }

        public class Handler : AsyncRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            protected override async Task<Result> HandleCore(Query message)
            {
                var users = await db.Users.OrderBy(u => u.DisplayName)
                    .ProjectToPagedListAsync<Result.User>(message.PageNumber ?? 1, Pagination.PageSize);
                return new Result {Users = users};
            }
        }
    }
}