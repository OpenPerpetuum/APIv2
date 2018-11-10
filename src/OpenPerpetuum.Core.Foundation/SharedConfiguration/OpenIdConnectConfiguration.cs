using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.Core.Foundation.SharedConfiguration
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
