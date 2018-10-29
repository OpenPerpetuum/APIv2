using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Api.Models.PlayerAccount;
using OpenPerpetuum.Core.Foundation.Processing;
using System.Net;

namespace OpenPerpetuum.Api.Controllers
{
	[Route("api/[controller]")]
	public class PlayerAccountController : ApiControllerBase
	{
		public PlayerAccountController(ICoreContext coreContext) : base(coreContext)
		{
		}

		[HttpPost("register"), Authorize(Policy = "AdminClient")]
		public IActionResult CreateNewPlayer([FromBody] CreateNewPlayerModel request)
		{
			return StatusCode((int)HttpStatusCode.NotImplemented);
		}
	}
}
