namespace OpenPerpetuum.Core.SharedIdentity.Configuration
{
	public class OpenIdConnectConfiguration
	{
		public string AuthorisationPath
		{
			get;
			set;
		}
		public string LogoutPath
		{
			get;
			set;
		}

		public string TokenPath
		{
			get;
			set;
		}

		public string UserInfoPath
		{
			get;
			set;
		}

		public bool AllowInsecureHttp
		{
			get;
			set;
		}

		public bool EnableJWT
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
