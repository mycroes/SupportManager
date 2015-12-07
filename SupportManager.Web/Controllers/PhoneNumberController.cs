using System.Web.Mvc;
using MediatR;
using SupportManager.Web.Features.PhoneNumber;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Controllers
{
    public class PhoneNumberController : Controller
    {
        private readonly IMediator mediator;

        public PhoneNumberController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public ActionResult Index(PhoneNumberListQuery query)
        {
            var results = mediator.Send(query);

            return View(results);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(PhoneNumberCreateCommand command)
        {
            mediator.Send(command);

            return this.RedirectToActionJson("Index");
        }
    }
}