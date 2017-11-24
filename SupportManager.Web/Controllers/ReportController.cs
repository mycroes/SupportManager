using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Features.Report;

namespace SupportManager.Web.Controllers
{
    public class ReportController : Controller
    {
        private readonly IMediator mediator;

        public ReportController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Monthly(Monthly.Query query)
        {
            var res = await mediator.Send(query);

            return View(res);
        }
    }
}
