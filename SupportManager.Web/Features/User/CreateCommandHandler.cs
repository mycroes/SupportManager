using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class CreateCommandHandler : RequestHandler<CreateCommand>
    {
        private readonly SupportManagerContext db;

        public CreateCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override void Handle(CreateCommand request)
        {
            var user = new DAL.User {DisplayName = request.Name, Login = request.Name};

            db.Users.Add(user);
        }
    }
}