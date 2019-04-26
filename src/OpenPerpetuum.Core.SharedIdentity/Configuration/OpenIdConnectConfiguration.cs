namespace OpenPerpetuum.Core.SharedIdentity.Configuration
{
	public class OpenIdConnectConfiguration
	{
		public bool RequireHttpsMetadata
		{
			get;
			set;
		}

		public bool UseDeveloperKeys
		{
			get;
			set;
		}

		public string IdentityServer
		{
			get;
			set;
		}
	}
}
