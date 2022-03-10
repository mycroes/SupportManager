using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Infrastructure.ApiKey;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.Web.Api.Team;
using SupportManager.Web.Areas.Teams.Pages;
using Schedule = SupportManager.Web.Api.Team.Schedule;

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

        [HttpDelete("forward/{id}")]
        public async Task<IActionResult> DeleteForward(int id)
        {
            // Hack, move command out of Model
            await mediator.Send(new DeleteForwardModel.Command {Id = id});
            return Ok();
        }

        [HttpPost("schedule")]
        public async Task<IActionResult> Schedule([FromBody] Schedule.Command command)
        {
            await mediator.Send(command);
            return Ok();
        }

        [HttpGet("members/{id}")]
        public async Task<ActionResult<List<UserDetails>>> GetMembers(int id)
        {
            return await mediator.Send(new Members.Query(id));
        }

        [HttpPost("forward")]
        public async Task<IActionResult> Forward([FromBody] Forward.Command command)
        {
            await mediator.Send(command);
            return Ok();
        }
    }
}
