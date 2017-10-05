using MediatR;
using Microsoft.AspNetCore.Mvc;
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

        public IActionResult Index(PhoneNumberListQuery query)
        {
            var results = mediator.Send(query);

            return View(results);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(PhoneNumberCreateCommand command)
        {
            mediator.Send(command);

            return this.RedirectToActionJson("Index");
        }
    }
}