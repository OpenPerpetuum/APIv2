using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Authorisation
{
	// Note: This class is *always* a singleton. Don't inject scoped dependencies
	public sealed class AuthorisationProvider : OpenIdConnectServerProvider
	{
		private readonly ApplicationContext database;

		public AuthorisationProvider(ApplicationContext database)
		{
			this.database = database;
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
			
			if (!Guid.TryParse(context.ClientId, out Guid clientId)) clientId = Guid.Empty;

			IQueryProcessor queryProcessor = context.HttpContext.RequestServices.GetService(typeof(IQueryProcessor)) as IQueryProcessor;

			AccessClientModel accessClient = await Task.Run(() => queryProcessor.Process(new API_GetPermittedClientQuery
			{
				ClientId = clientId
			})?.SingleOrDefault() ?? AccessClientModel.DefaultValue);

			if (Equals(accessClient.ClientId, Guid.Empty))
			{
				context.Reject(
					error: OpenIdConnectConstants.Errors.InvalidClient,
					description: "Requests from this client are not authorised");
			}

			if (string.IsNullOrWhiteSpace(context.RedirectUri))
			{
				if (!context.IsRejected)
					context.Reject(
						error: OpenIdConnectConstants.Errors.InvalidRequest,
						description: "Required redirect_uri parameter was missing");
			}

			if (!Uri.TryCreate(context.RedirectUri, UriKind.Absolute, out Uri redirectUri))
			{
				if (!context.IsRejected)
					context.Reject(
						error: OpenIdConnectConstants.Errors.InvalidClient,
						description: "Invalid redirect_uri");
			}
			else if (!accessClient.RedirectUris.Select(ru => ru.ToString()).Where(rs => string.Equals(rs, context.RedirectUri, StringComparison.InvariantCultureIgnoreCase)).Any())
			{
				if (!context.IsRejected)
					context.Reject(
						error: OpenIdConnectConstants.Errors.InvalidClient,
						description: "Invalid redirect_uri");
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
	}
}
