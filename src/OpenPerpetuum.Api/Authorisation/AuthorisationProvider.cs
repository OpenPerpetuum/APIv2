using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.EntityFrameworkCore;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Authorisation
{
	// Note: This class is *always* a singleton. Don't inject scoped dependencies
	public sealed class AuthorisationProvider : OpenIdConnectServerProvider
	{
		private readonly ApplicationContext dbContext;

		public AuthorisationProvider(ApplicationContext dbContext)
		{
			this.dbContext = dbContext;
		}

		// Implement OnValidateAuthorizationRequest to support interactive flows (code/implicit/hybrid).
		public override async Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
		{
			/* This method must run in a 'time-constant' fashion.
			 * Even when authorisation has failed at a certain point
			 * the method must continue executing *as though it hasn't failed*
			 * This prevents malicious agents being able to determine "correctness"
			 * by checking how long it takes for the method to return. In a non-time-constant
			 * authorisation process, the more of the process you get correct, the longer it will take to return.
			 * Ideally, the time to return should always stay the same.
			 */
			
			if (!context.Request.IsAuthorizationCodeFlow())
			{
				context.Reject(
					error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
					description: "Only authorisation code flow is supported by this server");
				return; // Given that this is a protocol error I'm not too fussed about the early return
			}
			
			AccessClientModel accessClient = await GetApplicationAsync(context.ClientId, context.HttpContext.RequestAborted) ?? AccessClientModel.DefaultValue;

			if (Equals(accessClient.ClientId, Guid.Empty))
			{
				context.Reject(
					error: OpenIdConnectConstants.Errors.InvalidClient,
					description: "Requests from this client are not authorised");
			}

			if (!string.Equals(accessClient.RedirectUri, context.RedirectUri, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!context.IsRejected)
					context.Reject(
						error: OpenIdConnectConstants.Errors.InvalidClient,
#if DEBUG
						description: "redirect_uri mismatch");
#else
						description: "Requests from this client are not authorised");
#endif
			}

			// Confirm the validation
			if (!context.IsRejected)
				context.Validate();

			return;
		}

		// Implement OnValidateTokenRequest to support flows using the token endpoint
		// (code/refresh token/password/client credentials/custom grant).
		public override Task ValidateTokenRequest(ValidateTokenRequestContext context)
		{
			if (context.Request.IsAuthorizationCodeGrantType())
				context.Skip();
			else
				context.Reject(
					error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
					description: "Only authorisation code grant types are supported");

			return Task.FromResult(0);
		}

		private async Task<AccessClientModel> GetApplicationAsync(string identifier, CancellationToken cancellationToken)
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
