using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hangfire;
using MediatR;
using SupportManager.Contracts;
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

        public async Task<ActionResult> Register()
        {
            var command = new CreateCommand {Name = User.Identity.Name};
            await mediator.Send(command);

            return RedirectToAction("Index");
        }

        public ActionResult AddEmailAddress()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddEmailAddress(AddEmailAddressCommand command)
        {
            command.UserName = User.Identity.Name;
            command.VerificationUrlBuilder = x => Url.Action("VerifyEmailAddress", "User", x, Request.Url.Scheme);

            await mediator.Send(command);

            return this.RedirectToActionJson("Index");
        }

        public async Task<ActionResult> VerifyEmailAddress(VerifyEmailAddressCommand command)
        {
            var result = await mediator.Send(command);

            if (result.Success) return RedirectToAction("Index");

            throw new Exception("Failed to verify");
        }

        public ActionResult Send(int id, DateTime when)
        {
            BackgroundJob.Schedule<IForwarder>(x => x.ApplyScheduledForward(id), when);

            return new ContentResult {Content = "OK"};
        }
    }
}