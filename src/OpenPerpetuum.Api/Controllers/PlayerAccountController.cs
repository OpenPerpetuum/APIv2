using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Api.Models.PlayerAccount;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using System.Net;

namespace OpenPerpetuum.Api.Controllers
{
    [Route("api/[controller]"), Authorize(Policy = Scopes.Registration)] // All methods in this controller require that the consuming client has the Registration scope in their Bearer token. NOTE: A client can be either a User or an Application
	public class PlayerAccountController : ApiControllerBase
	{
		public PlayerAccountController(ICoreContext coreContext) : base(coreContext)
		{
		}

		[HttpGet("[action]")]
		public IActionResult NoAuthTest()
		{
			return Ok();
		}

		[HttpGet("[action]")]
		public IActionResult Test()
		{
			return Ok();
		}

		[HttpPost("register")]
		public IActionResult CreateNewPlayer([FromBody] CreateNewPlayerModel request)
		{
			return StatusCode((int)HttpStatusCode.NotImplemented);
		}
	}
}
