using System.Threading.Tasks;
using System.Web.Mvc;
using MediatR;
using SupportManager.DAL;
using SupportManager.Web.Features.Admin.User;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Areas.Admin.Controllers
{
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
        public async Task<ActionResult> Create(Create.Command command)
        {
            await mediator.Send(command);
            return this.RedirectToActionJson(c => c.Index(null));
        }

        public ActionResult Detail(User user)
        {
            return View();
        }
    }
}