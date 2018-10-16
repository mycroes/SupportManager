using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportManager.Api.Teams;
using SupportManager.Api.Users;
using SupportManager.Web.Infrastructure.ApiKey;
using SupportManager.Web.Api.User;

namespace SupportManager.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = ApiKeyAuthenticationDefaults.AuthenticationScheme)]
    public class UserController : ControllerBase
    {
        private readonly IMediator mediator;

        public UserController(IMediator mediator) => this.mediator = mediator;

        [HttpGet]
        public async Task<ActionResult<UserDetails>> MyDetails() =>
            await mediator.Send(new MyDetails.Query(User.Identity.Name));

        [HttpGet("teams")]
        public async Task<ActionResult<List<TeamDto>>> MyTeams() =>
            await mediator.Send(new MyTeams.Query(User.Identity.Name));
    }
}
