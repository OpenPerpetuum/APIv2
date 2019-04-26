using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OpenPerpetuum.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenPerpetuum.Core.SharedIdentity.Authorisation.Policy
{
    /// <summary>
    /// Permission based policy provider. This provides a replacement for the default provided ASP NET Core provider.
    /// If a policy is specified that cannot be handled by the provider, it will fallback to the ASP NET Core default provider
    /// that is configured using the ServiceCollection.AddAuthorization extension
    /// </summary>
    public class RequiresPermissionPolicyProvider : IAuthorizationPolicyProvider
    {
        const string POLICY_PREFIX = "RequiresPermission";
        /// <summary>
        /// Fallback Policy Provider
        /// </summary>
        public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }
        /// <summary>
        /// Returns the Fallback Provider Default Policy
        /// </summary>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => FallbackPolicyProvider.GetDefaultPolicyAsync();

        /// <summary>
        /// Constructs the policy provider and sets the default fallback provider configured with the options provided to the DI container.
        /// Do not instantiate this manually. This class must be registered as a singleton in the ASP NET DI container
        /// </summary>
        /// <param name="options"></param>
        public RequiresPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
        {
            FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        }

        /// <summary>
        /// Gets the policy specified by name. If this is a permissions attribute policy, it will parse the required permissions from the policy name.
        /// This is mandated by the way that ASP NET Policy Providers work
        /// </summary>
        /// <param name="policyName">The policy to validate</param>
        /// <returns></returns>
        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith($"{POLICY_PREFIX}", StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();
                policy.AddRequirements(new RequiresPermissionRequirement(GetPermissions(policyName)));

                return Task.FromResult(policy.Build());
            }

            return FallbackPolicyProvider.GetPolicyAsync(policyName);
        }

        private Permission[] GetPermissions(string policyName)
        {
            var permissionsStart = policyName.Substring(POLICY_PREFIX.Length);
            var permissions = permissionsStart.Split('|', StringSplitOptions.RemoveEmptyEntries);
            List<Permission> permissionList = new List<Permission>();
            foreach (string permission in permissions)
            {
                permissionList.Add(EnumHelpers<Permission>.GetValueFromName(permission));
            }

            return permissionList.ToArray();
        }
    }
}
