using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Api.DependencyInstallers;
using OpenPerpetuum.Api.Models.Authorisation;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.Security;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Controllers
{
	public class AuthorisationController : ApiControllerBase
	{
		private readonly IMemoryCache cache;

		public AuthorisationController(ICoreContext coreContext, IMemoryCache cache) : base(coreContext)
		{
			this.cache = cache;
		}

		[Authorize, HttpGet("~/connect/authorise")]
		public IActionResult Authorise(CancellationToken cancellationToken)
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

			AccessClientModel accessClient = GetApplication(request.ClientId) ?? AccessClientModel.DefaultValue;

			if (accessClient.ClientId == Guid.Empty)
				return View("Error", new ErrorViewModel
				{
					Error = OpenIdConnectConstants.Errors.InvalidClient,
					ErrorDescription = "Calling application not found in permissions database. Please contact the developer"
				});

			return View("Authorise", Tuple.Create(request, accessClient));
		}

		[Authorize, FormValueRequired("submit.Accept")]
		[HttpPost("~/connect/authorise"), ValidateAntiForgeryToken]
		public IActionResult Accept(CancellationToken cancellationToken)
		{
			var response = HttpContext.GetOpenIdConnectResponse();
			if (response != null)
			{
				return View("Error", response);
			}

			var request = HttpContext.GetOpenIdConnectRequest();
			if (request == null)
			{
				return View("Error", new OpenIdConnectResponse
				{
					Error = OpenIdConnectConstants.Errors.ServerError,
					ErrorDescription = "An internal error has occurred."
				});
			}

			// Create a new ClaimsIdentity containing the claims that
			// will be used to create an id_token, a token or a code.
			var identity = new ClaimsIdentity(
				OpenIdConnectServerDefaults.AuthenticationScheme,
				OpenIdConnectConstants.Claims.Name,
				OpenIdConnectConstants.Claims.Role);

			// Note: the "sub" claim is mandatory and an exception is thrown if this claim is missing.
			identity.AddClaim(new Claim(OpenIdConnectConstants.Claims.Subject, User.FindFirst(ClaimTypes.NameIdentifier).Value)
				.SetDestinations(
					OpenIdConnectConstants.Destinations.AccessToken,
					OpenIdConnectConstants.Destinations.IdentityToken));

			identity.AddClaim(new Claim(OpenIdConnectConstants.Claims.Name, User.FindFirst(ClaimTypes.Name).Value)
				.SetDestinations(
					OpenIdConnectConstants.Destinations.AccessToken,
					OpenIdConnectConstants.Destinations.IdentityToken));

			identity.AddClaim(new Claim(OpenIdConnectConstants.Claims.Issuer, "OpenPerpetuumAPIv2")
				.SetDestinations(
					OpenIdConnectConstants.Destinations.AccessToken,
					OpenIdConnectConstants.Destinations.IdentityToken));

			identity.AddClaim(new Claim(OpenIdConnectConstants.Claims.ClientId, request.ClientId)
				.SetDestinations(
					OpenIdConnectConstants.Destinations.AccessToken,
					OpenIdConnectConstants.Destinations.IdentityToken));

			var application = GetApplication(request.ClientId);
			if (application == null)
			{
				return View("Error", new OpenIdConnectResponse
				{
					Error = OpenIdConnectConstants.Errors.InvalidClient,
					ErrorDescription = "The specified client identifier is invalid."
				});
			}

			// Create a new authentication ticket holding the user identity.
			var ticket = new AuthenticationTicket(
				new ClaimsPrincipal(identity),
				new AuthenticationProperties(),
				OpenIdConnectServerDefaults.AuthenticationScheme);

			// Set the list of scopes granted to the client application.
			// Note: this sample always grants the "openid", "email" and "profile" scopes
			// when they are requested by the client application: a real world application
			// would probably display a form allowing to select the scopes to grant.
			ticket.SetScopes(new[]
			{
                /* openid: */ OpenIdConnectConstants.Scopes.OpenId,
                /* email: */ OpenIdConnectConstants.Scopes.Email,
                /* profile: */ OpenIdConnectConstants.Scopes.Profile,
                /* offline_access: */ OpenIdConnectConstants.Scopes.OfflineAccess
			}.Intersect(request.GetScopes()));

			// Set the resources servers the access token should be issued for.
			ticket.SetResources("OpenPerpetuumAPIv2");

			// Returning a SignInResult will ask ASOS to serialize the specified identity to build appropriate tokens.
			return SignIn(ticket.Principal, ticket.Properties, ticket.AuthenticationScheme);
		}

		[Authorize, FormValueRequired("submit.Deny")]
		[HttpPost("~/connect/authorise"), ValidateAntiForgeryToken]
		public IActionResult Deny(CancellationToken cancellationToken)
		{
			var response = HttpContext.GetOpenIdConnectResponse();
			if (response != null)
			{
				return View("Error", response);
			}

			// Notify ASOS that the authorization grant has been denied by the resource owner.
			// Note: OpenIdConnectServerHandler will automatically take care of redirecting
			// the user agent to the client application using the appropriate response_mode.
			return Challenge(OpenIdConnectServerDefaults.AuthenticationScheme);
		}

		[HttpGet("~/connect/logout")]
		public async Task<ActionResult> Logout(CancellationToken cancellationToken)
		{
			var response = HttpContext.GetOpenIdConnectResponse();
			if (response != null)
			{
				return View("Error", response);
			}

			// When invoked, the logout endpoint might receive an unauthenticated request if the server cookie has expired.
			// When the client application sends an id_token_hint parameter, the corresponding identity can be retrieved
			// using AuthenticateAsync or using User when the authorization server is declared as AuthenticationMode.Active.
			var result = await HttpContext.AuthenticateAsync(OpenIdConnectServerDefaults.AuthenticationScheme);

			var request = HttpContext.GetOpenIdConnectRequest();
			if (request == null)
			{
				return View("Error", new OpenIdConnectResponse
				{
					Error = OpenIdConnectConstants.Errors.ServerError,
					ErrorDescription = "An internal error has occurred."
				});
			}

			return View("Logout", Tuple.Create(request, result?.Principal));
		}

		[HttpPost("~/connect/logout")]
		[ValidateAntiForgeryToken]
		public ActionResult Logout()
		{
			// Returning a SignOutResult will ask the cookies middleware to delete the local cookie created when
			// the user agent is redirected from the external identity provider after a successful authentication flow
			// and will redirect the user agent to the post_logout_redirect_uri specified by the client application.
			return SignOut("ServerCookie", OpenIdConnectServerDefaults.AuthenticationScheme);
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

			UserModel user = QueryProcessor.Process(new GAME_AuthenticateAndGetUserDetailsQuery
			{
				Email = viewModel.Username,
				EncryptedPassword = viewModel.Password.ToLegacyShaString()
			});

			// Create the identity principal
			var userIdentity = new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.Name, viewModel.Username), new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()) }, "ServerCookie");
			ClaimsPrincipal principal = new ClaimsPrincipal(userIdentity);
			
			if (user == UserModel.Default)
				return Unauthorized();

			await HttpContext.SignInAsync(
					"ServerCookie",
					principal);

			if (string.IsNullOrWhiteSpace(viewModel.ReturnUrl))
				return NoContent();

			return Redirect(viewModel.ReturnUrl);
		}

		private AccessClientModel GetApplication(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
				return null;

			if (!Guid.TryParse(identifier, out Guid clientId))
				clientId = Guid.Empty;

			if (!cache.TryGetValue(CacheKeys.AccessClients, out ReadOnlyCollection<AccessClientModel> applications) || applications == null || applications.Count == 0)
				return AccessClientModel.DefaultValue;

			// Retrieve the application details corresponding to the requested client_id.
			return applications.SingleOrDefault(ap => ap.ClientId == clientId) ?? AccessClientModel.DefaultValue;
		}
	}
}
