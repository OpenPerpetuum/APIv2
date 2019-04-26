using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Authorisation.Queries;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using OpenPerpetuum.Core.SharedIdentity.Authorisation.Policy;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace OpenPerpetuum.Api.Authorisation
{
    /// <summary>
    /// Handles the authorisation request where a <see cref="RequiresPermissionRequirement"/> requirement is specified.
    /// This class must be registered in the ASP NET Core DI container
    /// </summary>
    public class RequiresPermissionHandler : AuthorizationHandler<RequiresPermissionRequirement>
    {
        private readonly IQueryProcessor Repository;
        /// <summary>
        /// The repository to lookup the required permissions
        /// </summary>
        /// <param name="repo"></param>
        public RequiresPermissionHandler(IQueryProcessor repo) => Repository = repo;

        /// <summary>
        /// Override for handling the permissions check
        /// </summary>
        /// <param name="context">Authorisation context of the current user</param>
        /// <param name="requirement">Attribute specified requirement</param>
        /// <returns></returns>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequiresPermissionRequirement requirement)
        {
            if (HasPermissions(context.User, requirement.RequiredPermissions))
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.CompletedTask;
        }

        private bool HasPermissions(ClaimsPrincipal user, Permission[] permissions)
        {
            if (!user.Identity.IsAuthenticated) return false;

            var accountId = user.Claims.Single(c => c.Type == JwtClaimTypes.Subject).Value;
            var userModel = Repository.Process(new GAME_FindUserByAccountIdQuery { AccountId = accountId });

            if (CheckPermission(userModel, Permission.OWNER))
                return true;

            foreach (var permission in permissions)
            {
                if (!CheckPermission(userModel, permission))
                    return false;
            }

            return true;
        }

        private bool CheckPermission(UserModel userModel, Permission permission)
        {
            return userModel.Permissions.ToList().Contains(permission);
        }
    }
}
