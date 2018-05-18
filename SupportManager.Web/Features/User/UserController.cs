using System;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Contracts;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Features.User
{
    public class UserController : Controller
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var query = new DetailsQuery { UserName = User.Identity.Name };
            var details = await mediator.Send(query);
            return View(details);
        }

        public IActionResult Welcome()
        {
            return View();
        }

        public async Task<IActionResult> Register()
        {
            var command = new CreateCommand {Name = User.Identity.Name};
            await mediator.Send(command);

            return RedirectToAction("Index");
        }

        public IActionResult AddEmailAddress()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddEmailAddress(AddEmailAddressCommand command)
        {
            command.UserName = User.Identity.Name;
            command.VerificationUrlBuilder = x => Url.Action("VerifyEmailAddress", "User", x, Request.Scheme);

            await mediator.Send(command);

            return this.RedirectToActionJson("Index");
        }

        public async Task<IActionResult> VerifyEmailAddress(VerifyEmailAddressCommand command)
        {
            var result = await mediator.Send(command);

            if (result.Success) return RedirectToAction("Index");

            throw new Exception("Failed to verify");
        }

        public IActionResult Send(int id, DateTime when)
        {
            BackgroundJob.Schedule<IForwarder>(x => x.ApplyScheduledForward(id, null), when);

            return new ContentResult {Content = "OK"};
        }
    }
}