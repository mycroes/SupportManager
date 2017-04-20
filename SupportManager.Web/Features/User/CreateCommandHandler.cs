using SupportManager.DAL;
using SupportManager.Web.Infrastructure.CommandProcessing;

namespace SupportManager.Web.Features.User
{
    public class CreateCommandHandler : RequestHandler<CreateCommand>
    {
        private readonly SupportManagerContext db;

        public CreateCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override void HandleCore(CreateCommand message)
        {
            var user = new DAL.User {DisplayName = message.Name, Login = message.Name};

            db.Users.Add(user);
        }
    }
}