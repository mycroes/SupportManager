using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Features.Admin.Team;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Teams.Home
{
    public class HomeController : BaseController
    {
        private readonly IMediator mediator;

        public HomeController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Index(Index.Query query)
        {
            var vm = await mediator.Send(query);
            return View(vm);
        }

        public async Task<ActionResult> EditForward(EditForward.Query query)
        {
            var command = await mediator.Send(query);
            return View(command);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditForward(EditForward.Command command)
        {
            await mediator.Send(command);
            return this.RedirectToActionJson("Index");
        }

        public async Task<IActionResult> SetForward(SetForward.Command command)
        {
            await mediator.Send(command);
            return RedirectToAction("Index");
        }

        public ActionResult ScheduleForward()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ScheduleForward(ScheduleForward.Command command)
        {
            await mediator.Send(command);
            return this.RedirectToActionJson("Index");
        }

        public async Task<IActionResult> DeleteForward(DeleteForward.Request request)
        {
            var model = await mediator.Send(request);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteForward(DeleteForward.Command command)
        {
            await mediator.Send(command);
            return this.RedirectToActionJson("Index");
        }
    }
}