using IdentityServer4.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.IdentityServer.ViewModel.Home;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.Controllers
{
	[SecurityHeaders]
	[AllowAnonymous]
	public class HomeController : ApiControllerBase
	{
		private readonly IIdentityServerInteractionService identityService;
		private readonly IHostingEnvironment environment;

		public HomeController(ICoreContext coreContext, IIdentityServerInteractionService identityService, IHostingEnvironment environment) : base(coreContext)
		{
			this.environment = environment;
			this.identityService = identityService;
		}

		public IActionResult Index()
		{
			if (environment.IsDevelopment())
				return View();

			return NotFound();
		}

		public async Task<IActionResult> Error(string errorId)
		{
			var vm = new ErrorViewModel();

			var message = await identityService.GetErrorContextAsync(errorId);
			if (message != null)
			{
				vm.Error = message;

				if (!environment.IsDevelopment())
					message.ErrorDescription = null;
			}

			return View("Error", vm);
		}
	}
}
