using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Admin.User
{
    [Area("Admin")]
    [Authorize]
    public class UserController : Controller
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        // GET: Admin/User
        public async Task<ActionResult> Index(Index.Query query)
        {
            var vm = await mediator.Send(query);
            return View(vm);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Create.Command command)
        {
            await mediator.Send(command);
            return this.RedirectToActionJson("Index");
        }

        public ActionResult Detail(DAL.User user)
        {
            return View();
        }
    }
}