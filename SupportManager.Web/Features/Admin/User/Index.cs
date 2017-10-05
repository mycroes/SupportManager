using System.Linq;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Infrastructure;
using X.PagedList;

namespace SupportManager.Web.Features.Admin.User
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

        public class Handler : IRequestHandler<Query, Result>
        {
            private readonly SupportManagerContext db;

            public Handler(SupportManagerContext db)
            {
                this.db = db;
            }

            public Result Handle(Query message)
            {
                var users = db.Users.OrderBy(u => u.DisplayName)
                    .ProjectToPagedList<Result.User>(message.PageNumber ?? 1, Pagination.PageSize);
                return new Result {Users = users};
            }
        }
    }
}