using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using Microsoft.Extensions.Caching.Memory;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Foundation.Security;
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

			return Task.CompletedTask;
		}

		// Implement OnValidateTokenRequest to support flows using the token endpoint
		// (code/refresh token/password/client credentials/custom grant).
		public override Task ValidateTokenRequest(ValidateTokenRequestContext context)
		{
			if (context.Request.IsAuthorizationCodeGrantType())
				context.Skip();
			else if (context.Request.IsClientCredentialsGrantType())
			{
				AccessClientModel client = GetApplication(context.ClientId);
				// This next step only really services for slowing down the process and also hiding the real secret key lengths
				byte[] salt = Cryptography.CreateRandomBytes(32);
				byte[] appSecret = Cryptography.HashPassword(client.Secret, salt);
				byte[] incomingSecret = Cryptography.HashPassword(context.ClientSecret, salt);

				bool correctSecret = true;

				// Cos why not on one line? Also this has to be done to hide length differences. No matter how long a string they send in, it always compares it in its entirety
				// This breaks the time constant rule though "kind of". We only test the length of the incoming string, but we test every character. So if they send in a short string
				// then it will take less time than a long string, however, they cannot glean the length of the correct string because the time is constant to the length of the incoming string
				// and not the length of the actual string.
				// STRICTLY SPEAKING we only request 32 bytes from the hash algorithm, so they'll always be the same length regardless, but just in case...
				for (int i = 0; i < incomingSecret.Length; i++) correctSecret &= (incomingSecret[i] == ((i >= appSecret.Length) ? appSecret[appSecret.Length - 1] : appSecret[i]));

				bool isError = !correctSecret;

				if (client == AccessClientModel.DefaultValue)
				{
					isError = true;
					context.Reject(
						error: OpenIdConnectConstants.Errors.InvalidClient,
						description: "Requests from this client are not authorised");
				}

				if (!isError)
					context.Validate();
			}
			else
				context.Reject(
					error: OpenIdConnectConstants.Errors.UnsupportedGrantType,
					description: "Only authorisation code grant types are supported");

			return Task.CompletedTask;
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
