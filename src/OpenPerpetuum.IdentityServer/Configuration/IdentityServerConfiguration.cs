using IdentityServer4.Models;
using OpenPerpetuum.Core.Foundation.SharedConfiguration;
using System.Collections.Generic;

namespace OpenPerpetuum.IdentityServer.Configuration
{
	public class IdentityServerConfiguration
	{
		public static IEnumerable<ApiResource> GetApiResources()
		{
			return new List<ApiResource>
			{
				new ApiResource(OpenPerpetuumScopes.Registration, "Open Perpetuum Registration"),
				new ApiResource(OpenPerpetuumScopes.Killboard, "Open Perpetuum Killboard"),
				new ApiResource(OpenPerpetuumScopes.API_Name, "Open Perpetuum API")
			};
		}
	}
}
