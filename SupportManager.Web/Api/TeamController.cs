using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Web.Infrastructure;
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
            if (!User.CanViewTeam(id)) return NotFound();

            return await mediator.Send(new Status.Query(id));
        }

        [HttpGet("schedule/{id}")]
        public async Task<ActionResult<List<ForwardRegistration>>> GetSchedule(int id,
            [FromQuery, Range(1, 1000)] int limit = 10)
        {
            if (!User.CanViewTeam(id)) return NotFound();

            return await mediator.Send(new Schedule.Query(id, limit));
        }

        [HttpDelete("forward/{id}")]
        public async Task<IActionResult> DeleteForward(int id)
        {
            // Hack, move command out of Model
            var command = await mediator.Send(new DeleteForwardModel.Query(id));
            if (command == null || !User.IsTeamMember(command.TeamId)) return NotFound();

            await mediator.Send(command);
            return Ok();
        }

        [HttpPost("schedule")]
        public async Task<ActionResult<int>> Schedule([FromBody] Schedule.Command command)
        {
            if (!User.IsTeamMember(command.TeamId)) return NotFound();

            return await mediator.Send(command);
        }

        [HttpGet("members/{id}")]
        public async Task<ActionResult<List<UserDetails>>> GetMembers(int id)
        {
            if (!User.CanViewTeam(id)) return NotFound();

            return await mediator.Send(new Members.Query(id));
        }

        [HttpPost("forward")]
        public async Task<IActionResult> Forward([FromBody] Forward.Command command)
        {
            if (!User.IsTeamMember(command.TeamId)) return NotFound();

            await mediator.Send(command);
            return Ok();
        }
    }
}
