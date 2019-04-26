using Microsoft.AspNetCore.Authorization;

namespace OpenPerpetuum.Core.SharedIdentity.Authorisation.Policy
{
    /// <summary>
	/// Specifies a Permissions authorisation requirement that can be checked via the authorisation handler
	/// </summary>
	public class RequiresPermissionRequirement : IAuthorizationRequirement
    {
        /// <summary>
        /// The required permissions specified by the attribute
        /// </summary>
        public Permission[] RequiredPermissions
        { get; private set; }

        /// <summary>
        /// Constructs the requirement
        /// </summary>
        /// <param name="permissions">Permissions required by the attribute</param>
        public RequiresPermissionRequirement(params Permission[] permissions) => RequiredPermissions = permissions;
    }
}
