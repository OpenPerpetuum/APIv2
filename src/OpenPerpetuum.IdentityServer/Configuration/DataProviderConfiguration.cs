using OpenPerpetuum.Core.DataServices;

namespace OpenPerpetuum.IdentityServer.Configuration
{
	public class DataProviderConfiguration
	{
		public DatabaseProviderConfiguration[] Databases
		{
			get;
			set;
		}
	}
}
