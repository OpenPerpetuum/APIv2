using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenPerpetuum.Api.Authorisation;
using OpenPerpetuum.Api.Models.Authorisation;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Api.Controllers
{
	public class AuthorisationController : ApiControllerBase
	{
		private readonly ApplicationContext dbContext;

		public AuthorisationController(ICoreContext coreContext, ApplicationContext context) : base(coreContext)
		{
			dbContext = context;
		}

		[Authorize, HttpGet("~/connect/authorise")]
		public async Task<IActionResult> Authorise(CancellationToken cancellationToken)
		{
			// Extract the auth request from the context
			var response = HttpContext.GetOpenIdConnectResponse();

			if (response != null)
				return View("Error", response);

			// Note: ASOS implicitly ensures that an application corresponds to the client_id
			// specified in the authorization request by calling ValidateAuthorizationRequest.
			// In theory, this null check shouldn't be needed, but a race condition could occur
			// if you manually removed the application from the database after the initial check.

			var request = HttpContext.GetOpenIdConnectRequest();
			if (request == null)
				return View("Error", new OpenIdConnectResponse
				{
					Error = OpenIdConnectConstants.Errors.ServerError,
					ErrorDescription = "Internal error"
				});

			AccessClientModel accessClient = await GetApplicationAsync(request.ClientId, cancellationToken) ?? AccessClientModel.DefaultValue;

			if (accessClient.ClientId == Guid.Empty)
				return View("Error", new ErrorViewModel
				{
					Error = OpenIdConnectConstants.Errors.InvalidClient,
					ErrorDescription = "Calling application not found in permissions database. Please contact the developer"
				});

			return View(new AuthoriseViewModel
			{
				ApplicationName = accessClient.FriendlyName,
				Parameters = response.GetParameters().ToDictionary(ks => ks.Key, ele => ele.Value.ToString()),
				Scope = response.Scope
			});
		}

		[HttpGet("~/authorisation/login")]
		public IActionResult Login(string returnUrl = "")
		{
			var model = new LoginViewModel { ReturnUrl = returnUrl };
			return View(model);
		}

		[HttpPost("~/authorisation/login"), ValidateAntiForgeryToken]
		public async Task<IActionResult> Login([FromForm] LoginViewModel viewModel)
		{
			if (!ModelState.IsValid)
				return BadRequest();
			await HttpContext.SignOutAsync("ServerCookie");
			// Check the username and password just testing for now so allow it

			// Create the identity principal
			var userIdentity = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, viewModel.Username) }, "ServerCookie");
			ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
			await HttpContext.SignInAsync(
					"ServerCookie",
					principal);
			/*,
					new AuthenticationProperties
					{
						AllowRefresh = false,
						ExpiresUtc = GenericContext.CurrentDateTime.AddHours(8),
						IsPersistent = false,
						IssuedUtc = GenericContext.CurrentDateTime
					});
			*/

			if (string.IsNullOrWhiteSpace(viewModel.ReturnUrl))
				return NoContent();

			return Redirect(viewModel.ReturnUrl);
		}

		protected virtual async Task<AccessClientModel> GetApplicationAsync(string identifier, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(identifier))
				return null;

			if (!Guid.TryParse(identifier, out Guid clientId))
				clientId = Guid.Empty;

			// Retrieve the application details corresponding to the requested client_id.
			return await dbContext.Applications.Where(application => application.ClientId == clientId).SingleOrDefaultAsync(cancellationToken);
		}
	}
}
