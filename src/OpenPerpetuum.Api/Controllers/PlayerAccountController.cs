using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Api.Models.PlayerAccount;
using OpenPerpetuum.Core.Foundation.Processing;
using System.Net;
using System.Threading.Tasks;
using static OpenPerpetuum.Core.SharedIdentity.Configuration.IdentityConfig;

namespace OpenPerpetuum.Api.Controllers
{
	[Route("api/[controller]")]
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

		[HttpGet("[action]"), Authorize(Policy = Scopes.Registration)]
		public IActionResult Test()
		{
			return Ok();
		}

		[HttpPost("register"), Authorize(Policy = Scopes.Registration)]
		public IActionResult CreateNewPlayer([FromBody] CreateNewPlayerModel request)
		{
			return StatusCode((int)HttpStatusCode.NotImplemented);
		}
	}
}
