using System;
using System.Web.Mvc;
using MediatR;
using SupportManager.Web.Features.User;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Controllers
{
    public class UserController : Controller
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public ActionResult Index()
        {
            var query = new DetailsQuery { UserName = User.Identity.Name };
            var details = mediator.Send(query);
            return View(details);
        }

        public ActionResult Welcome()
        {
            return View();
        }

        public ActionResult Register()
        {
            var command = new CreateCommand {Name = User.Identity.Name};
            mediator.Send(command);

            return RedirectToAction("Index");
        }

        public ActionResult AddEmailAddress()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddEmailAddress(AddEmailAddressCommand command)
        {
            command.UserName = User.Identity.Name;
            command.VerificationUrlBuilder = x => Url.Action("VerifyEmailAddress", "User", x, Request.Url.Scheme);

            mediator.Send(command);

            return this.RedirectToActionJson("Index");
        }

        public ActionResult VerifyEmailAddress(VerifyEmailAddressCommand command)
        {
            var result = mediator.Send(command);

            if (result.Success) return RedirectToAction("Index");

            throw new Exception("Failed to verify");
        }
    }
}