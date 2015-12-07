using System.Linq;
using AutoMapper;
using MediatR;
using SupportManager.DAL;

namespace SupportManager.Web.Features.User
{
    public class AddPhoneNumberCommandHandler : RequestHandler<AddPhoneNumberCommand>
    {
        private readonly SupportManagerContext db;

        public AddPhoneNumberCommandHandler(SupportManagerContext db)
        {
            this.db = db;
        }

        protected override void HandleCore(AddPhoneNumberCommand message)
        {
            var user = db.Users.WhereUserNameIs(message.UserName).Single();

            user.PhoneNumbers.Add(Mapper.Map<UserPhoneNumber>(message.PhoneNumber));
        }
    }
}