using Microsoft.AspNetCore.Authorization;
using OpenPerpetuum.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPerpetuum.Core.SharedIdentity.Authorisation.Policy
{
    /// <summary>
	/// Requires that an authenticated user has the specified permission associated with their account
	/// </summary>
	public class RequiresPermissionAttribute : AuthorizeAttribute
    {
        const string POLICY_PREFIX = "RequiresPermission";

        /// <summary>
        /// Pass-thru data handler. Use to get a list of permissions required by this attribute
        /// </summary>
        /// <param name="permissions"></param>
        public RequiresPermissionAttribute(params Permission[] permissions) => RequiredPermissions = permissions;

        /// <summary>
        /// An array containing the permissions required by the attribute
        /// </summary>
        public Permission[] RequiredPermissions
        {
            get
            {
                var permissionsStart = Policy.Substring(POLICY_PREFIX.Length);
                var permissions = permissionsStart.Split('|', StringSplitOptions.RemoveEmptyEntries);
                List<Permission> permissionList = new List<Permission>();
                foreach (string permission in permissions)
                {
                    permissionList.Add(EnumHelpers<Permission>.GetValueFromName(permission));
                }

                return permissionList.ToArray();
            }
            set
            {
                StringBuilder sb = new StringBuilder();
                foreach (var permission in value)
                {
                    sb.Append($"|{EnumHelpers<Permission>.GetDisplayNameValue(permission)}");
                }

                Policy = $"{POLICY_PREFIX}{sb.ToString()}";
            }
        }
    }
}
