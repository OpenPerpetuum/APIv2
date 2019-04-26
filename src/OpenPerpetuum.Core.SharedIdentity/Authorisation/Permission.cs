using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OpenPerpetuum.Core.SharedIdentity.Authorisation
{
    /// <summary>
    /// Permissions for the authorisation system.
    /// Need to look at how PERP stores admin levels and whether
    /// we can re-use it for this.
    /// </summary>
    public enum Permission
    {
        NOT_USED,
        [Display(Name = "op.permit.owner", Description = "User is a system owner")]
        OWNER
    }
}
