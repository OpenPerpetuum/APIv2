﻿using IdentityModel;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace OpenPerpetuum.Core.SharedIdentity.Configuration
{
	// I'd rather configure this using outward reflection or self-registration
	// but this will do for now
	public static class IdentityConfig
	{
		public const string API_Name = "OPAPI";

		public static class Scopes
		{
			public const string Registration = "OP_REG";
			public const string Killboard = "OP_KILLBOARD";
		}

		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new[]
			{
				new ApiResource
				{
					Name = API_Name,
					DisplayName = "Open Perpetuum API",
					UserClaims =
					{
						JwtClaimTypes.Audience,
						JwtClaimTypes.Name,
						JwtClaimTypes.Email,
						JwtClaimTypes.EmailVerified,
						JwtClaimTypes.ClientId
					},
					Scopes =
					{
						new Scope
						{
							Name = Scopes.Registration,
							DisplayName = "Access to registration resources"
						},
						new Scope
						{
							Name = Scopes.Killboard,
							DisplayName = "Access to killboard and leaderboards"
						}
						// If we wanted to add further scoped resources we can. We can control access to resources with Scopes via the Authorize attribute
					}
				}
				// We can add further APIs in here and run them as separate services
			};
		}

		public static IEnumerable<IdentityResource> GetIdentityResources()
		{
			return new List<IdentityResource>
			{
				new IdentityResources.OpenId(),
				new IdentityResources.Profile()
			};
		}
	}
}
