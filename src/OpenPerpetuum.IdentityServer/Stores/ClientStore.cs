using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.SharedConfiguration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace OpenPerpetuum.IdentityServer.Stores
{
	public class ClientStore : IClientStore
	{
		private readonly IdentityServerOptions options;
		private readonly IQueryProcessor queryProcessor;
		private readonly ILogger logger;

		/// <summary>
		/// Initialises a new instance of the <see cref="CachingClientStore{T}"/> class
		/// </summary>
		/// <param name="options">IDS4 options</param>
		/// <param name="innerStore">The actual client store service</param>
		/// <param name="cache">The cache holding the clients</param>
		/// <param name="logger">Logging output</param>
		public ClientStore(IdentityServerOptions options, IQueryProcessor queryProcessor, ILogger<ClientStore> logger)
		{
			this.options = options;
			this.queryProcessor = queryProcessor;
			this.logger = logger;
		}

		public async Task<Client> FindClientByIdAsync(string clientId)
		{
			if (!Guid.TryParse(clientId, out Guid clientGuid))
				return null;

			// Note to self; make the processors async ffs
			AccessClientModel client = await Task.Run(() =>
			{
				return queryProcessor.Process(new API_GetPermittedClientQuery
				{
					ClientId = clientGuid
				})?.SingleOrDefault();
			});

			return new Client
			{
				ClientId = clientId,
				ClientName = client.FriendlyName,
				ClientSecrets = new List<Secret> { new Secret(client.Secret) },
				Enabled = true,
				RedirectUris = new List<string> { client.RedirectUri },
				AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
				AllowedScopes = new List<string>
				{
					StandardScopes.OpenId,
					StandardScopes.OfflineAccess,
					StandardScopes.Profile,
					StandardScopes.Email, /* We should add in the custom API scopes here. This needs to be stored in the DB. For now, allow OP_REG and OP_KILLBOARD */
					OpenPerpetuumScopes.Registration,
					OpenPerpetuumScopes.Killboard,
					OpenPerpetuumScopes.API_Name

				}
			};
		}
	}
}
