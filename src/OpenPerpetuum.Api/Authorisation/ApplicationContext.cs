using Microsoft.EntityFrameworkCore;
using OpenPerpetuum.Core.Authorisation.Models;

namespace OpenPerpetuum.Api.Authorisation
{
	/// <summary>
	/// This DbContext should ONLY be used as in-memory cache for the OpenId Connect Server
	/// Refresh it with Applications from DB every 15 minutes
	/// </summary>
	public class ApplicationContext : DbContext
	{
		public ApplicationContext(DbContextOptions options) : base(options) { }

		public DbSet<AccessClientModel> Applications { get; set; }
	}
}
