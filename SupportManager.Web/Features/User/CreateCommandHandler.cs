using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class CreateCommandHandler : IRequestHandler<CreateCommand>
    {
        private readonly SupportManagerContext db;

        public CreateCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        public void Handle(CreateCommand message)
        {
            var user = new DAL.User {DisplayName = message.Name, Login = message.Name};

            db.Users.Add(user);
        }
    }
}