using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.DAL;
using SupportManager.Web.Features.Admin.User;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        public ActionResult Detail(User user)
        {
            return View();
        }
    }
}