namespace OpenPerpetuum.Core.Authorisation.Models
{
	// Ripped from the OP Server AccessLevel enum
	public enum AccessLevel : int // This is uint in the server code but int in the database. Database should be changed but there's a long list...
	{
		NotDefined = 0,
		Normal = 2,
		GameAdmin = 6,
		ToolAdmin = 14,
		Owner = 30,
		AllAdmin = ToolAdmin | GameAdmin,
		Admin = AllAdmin
	}
}
