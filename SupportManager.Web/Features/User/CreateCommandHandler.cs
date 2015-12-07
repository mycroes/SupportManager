using AutoMapper;
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

        protected override void HandleCore(CreateCommand message)
        {
            var user = Mapper.Map<DAL.User>(message);
            db.Users.Add(user);
        }
    }
}