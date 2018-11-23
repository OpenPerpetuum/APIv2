using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.IdentityServer.ViewModel.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.Controllers
{
	[SecurityHeaders, Authorize]
	public class DiagnosticsController : ApiControllerBase
	{
		public DiagnosticsController(ICoreContext coreContext) : base(coreContext)
		{
		}

		public async Task<IActionResult> Index()
		{
			var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };
			if (!localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
				return NotFound();

			var model = new DiagnosticsViewModel(await HttpContext.AuthenticateAsync());

			return View(model);
		}
	}
}
