using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Infrastructure.ApiKey;
using SupportManager.Api.Teams;
using SupportManager.Web.Api.Team;

namespace SupportManager.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
    public class TeamController : ControllerBase
    {
        private readonly IMediator mediator;

        public TeamController(IMediator mediator) => this.mediator = mediator;

        [HttpGet("status/{id}")]
        public async Task<ActionResult<TeamStatus>> Status(int id)
        {
            return await mediator.Send(new Status.Query(id));
        }

        [HttpGet("schedule/{id}")]
        public async Task<ActionResult<List<ForwardRegistration>>> GetSchedule(int id)
        {
            return await mediator.Send(new Schedule.Query(id));
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> Schedule([FromBody] Schedule.Command command)
        {
            await mediator.Send(command);
            return Ok();
        }
    }
}
