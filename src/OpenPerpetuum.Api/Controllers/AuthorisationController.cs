using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using OpenPerpetuum.Api.Models.Authorisation;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Api.Controllers
{
	public class AuthorisationController : ApiControllerBase
	{
		public AuthorisationController(ICoreContext coreContext) : base(coreContext)
		{
		}

		[Authorize, HttpGet("~/connect/authorise")]
		public IActionResult Authorise(CancellationToken cancellationToken)
		{
			// Extract the auth request from the context
			var request = HttpContext.GetOpenIdConnectRequest();

			// Note: ASOS implicitly ensures that an application corresponds to the client_id
			// specified in the authorization request by calling ValidateAuthorizationRequest.
			// In theory, this null check shouldn't be needed, but a race condition could occur
			// if you manually removed the application from the database after the initial check.

			if (!Guid.TryParse(request.ClientId, out Guid clientId)) clientId = Guid.Empty;

			AccessClientModel accessClient = QueryProcessor.Process(new API_GetPermittedClientQuery
			{
				ClientId = clientId
			}).SingleOrDefault() ?? AccessClientModel.DefaultValue;

			if (accessClient.ClientId == Guid.Empty)
				return View("Error", new ErrorViewModel
				{
					Error = OpenIdConnectConstants.Errors.InvalidClient,
					ErrorDescription = "Calling application not found in permissions database. Please contact the developer"
				});

			return View(new AuthoriseViewModel
			{
				ApplicationName = accessClient.FriendlyName,
				Parameters = request.GetParameters().ToDictionary(ks => ks.Key, ele => ele.Value.ToString()),
				Scope = request.Scope
			});
		}
	}
}
