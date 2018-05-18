using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Features.Report;

namespace SupportManager.Web.Areas.Teams.Report
{
    public class ReportController : BaseController
    {
        private readonly IMediator mediator;

        public ReportController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public async Task<IActionResult> Index(int teamId)
        {
            var query = new Monthly.Query {TeamId = teamId, Year = DateTime.Now.Year, Month = DateTime.Now.Month};
            var res = await mediator.Send(query);

            return View(nameof(Monthly), res);
        }

        [HttpGet("Teams/{teamId}/Report/{year}/{month}")]
        public async Task<IActionResult> Monthly(Monthly.Query query)
        {
            var res = await mediator.Send(query);
            return View(res);
        }
    }
}
