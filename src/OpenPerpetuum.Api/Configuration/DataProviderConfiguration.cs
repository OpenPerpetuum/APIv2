using OpenPerpetuum.Core.DataServices;

namespace OpenPerpetuum.Api.Configuration
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
