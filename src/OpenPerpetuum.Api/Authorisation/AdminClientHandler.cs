using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using OpenPerpetuum.Api.Configuration;
using OpenPerpetuum.Core.Authorisation.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Authorisation
{
	public class AdminClientHandler : AuthorizationHandler<AdminClientRequirement>
	{
		private readonly IMemoryCache memCache;

		public AdminClientHandler(IMemoryCache memCache)
		{
			this.memCache = memCache;
		}

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminClientRequirement requirement)
		{
			ReadOnlyCollection<AccessClientModel> adminApps = GetApplications();
			foreach (AccessClientModel model in adminApps)
				if (context.User.HasClaim(c => c.Type == OpenIdConnectConstants.Claims.ClientId && c.Value == model.ClientId.ToString())) context.Succeed();

			return Task.CompletedTask;
		}

		protected virtual ReadOnlyCollection<AccessClientModel> GetApplications()
		{
			if (!memCache.TryGetValue(CacheKeys.AccessClients, out ReadOnlyCollection<AccessClientModel> applications) || applications == null || applications.Count == 0)
				return new List<AccessClientModel>().AsReadOnly();

			// Retrieve the application details corresponding to the requested client_id.
			return applications.Where(ap => ap.IsAdministratorApp).ToList().AsReadOnly() ?? new List<AccessClientModel>().AsReadOnly();
		}
	}
}
