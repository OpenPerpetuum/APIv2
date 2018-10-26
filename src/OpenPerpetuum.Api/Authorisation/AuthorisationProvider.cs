using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.Extensions.Caching.Memory;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Core.Authorisation.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Authorisation
{
	// Note: This class is *always* a singleton. Don't inject scoped dependencies
	public sealed class AuthorisationProvider : OpenIdConnectServerProvider
	{
		private readonly IMemoryCache dbContext;

		public AuthorisationProvider(IMemoryCache dbContext)
		{
			this.dbContext = dbContext;
		}

		// Implement OnValidateAuthorizationRequest to support interactive flows (code/implicit/hybrid).
		public override Task ValidateAuthorizationRequest(ValidateAuthorizationRequestContext context)
		{
			/* This method must run in a 'time-constant' fashion.
			 * Even when authorisation has failed at a certain point
			 * the method must continue executing *as though it hasn't failed*
			 * This prevents malicious agents being able to determine "correctness"
			 * by checking how long it takes for the method to return. In a non-time-constant
			 * authorisation process, the more of the process you get correct, the longer it will take to return.
			 * Ideally, the time to return should always stay the same.
			 */
			
			bool isError = false;

			if (!context.Request.IsAuthorizationCodeFlow())
			{
				context.Reject(
					error: OpenIdConnectConstants.Errors.UnsupportedResponseType,
					description: "Only authorisation code flow is supported by this server");
				return Task.FromResult(0); // Given that this is a protocol error I'm not too fussed about the early return
			}
			
			AccessClientModel accessClient = GetApplication(context.ClientId) ?? AccessClientModel.DefaultValue;

			if (Equals(accessClient.ClientId, Guid.Empty))
			{
				context.Reject(
					error: OpenIdConnectConstants.Errors.InvalidClient,
					description: "Requests from this client are not authorised");
				isError = true;
			}

			if (!string.Equals(accessClient.RedirectUri, context.RedirectUri, StringComparison.InvariantCultureIgnoreCase))
			{
				if (!isError)
					context.Reject(
						error: OpenIdConnectConstants.Errors.InvalidClient,
#if DEBUG
						description: "redirect_uri mismatch");
#else
						description: "Requests from this client are not authorised");
#endif
				isError = true;
			}

			if (!isError)
				context.Validate();

			return Task.FromResult(0);
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

		private AccessClientModel GetApplication(string identifier)
		{
			if (string.IsNullOrWhiteSpace(identifier))
				return null;

			if (!Guid.TryParse(identifier, out Guid clientId))
				clientId = Guid.Empty;

			if (!dbContext.TryGetValue(CacheKeys.AccessClients, out ReadOnlyCollection<AccessClientModel> applications) || applications == null || applications.Count == 0)
				return AccessClientModel.DefaultValue;

			// Retrieve the application details corresponding to the requested client_id.
			return applications.SingleOrDefault(ap => ap.ClientId == clientId) ?? AccessClientModel.DefaultValue;
		}
	}
}
