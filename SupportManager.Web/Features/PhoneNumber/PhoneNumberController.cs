using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Infrastructure;

namespace SupportManager.Web.Features.PhoneNumber
{
    public class PhoneNumberController : Controller
    {
        private readonly IMediator mediator;

        public PhoneNumberController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Index(PhoneNumberListQuery query)
        {
            var results = await mediator.Send(query);

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