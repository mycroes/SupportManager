using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Features.User;

namespace SupportManager.Web.Features.Home
{
    public class HomeController : Controller
    {
        private readonly IMediator mediator;

        public HomeController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Index()
        {
            var result = await mediator.Send(new UserExistsQuery {UserName = User.Identity.Name});
            return RedirectToAction(result.IsExistingUser ? "Index" : "Welcome", "User");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}