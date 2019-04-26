using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using OpenPerpetuum.Core.SharedIdentity.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace OpenPerpetuum.IdentityServer.Configuration
{
    public class IdentityServerConfiguration
    {
        // scopes define the resources in your system
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return IdentityConfig.GetIdentityResources();
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return IdentityConfig.GetApiResources();
        }

        // clients want to access resources (aka scopes)
        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            var clients = new List<Client>();
            var installConfig = configuration.GetSection("FirstRunSetup");
            var webClientConfig = installConfig.GetSection("PerpetuumWebClient");

            var allowedScopes = GetApiResources().Single(ar => ar.Name == IdentityConfig.API_Name).Scopes.Select(s => s.Name).Union(GetIdentityResources().Select(ir => ir.Name)).ToList();
            var initialClient = new Client
            {
                ClientId = webClientConfig.GetValue<string>("ClientId"),
                ClientName = "Local Development Client",
                AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                ClientSecrets = { new Secret(webClientConfig.GetValue<string>("Secret").Sha256()) },
                RedirectUris = webClientConfig.GetSection("RedirectUris").Get<string[]>().ToList(),
                PostLogoutRedirectUris = webClientConfig.GetSection("PostLogoutRedirectUris").Get<string[]>().ToList(),
                BackChannelLogoutSessionRequired = false,
                FrontChannelLogoutSessionRequired = false,
                AllowedScopes = allowedScopes,
                AllowOfflineAccess = true,
                RequireConsent = false,
                IdentityTokenLifetime = 1200
            };

            clients.Add(initialClient);
            return clients;
        }
    }
}
