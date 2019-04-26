﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenPerpetuum.IdentityServer.ViewModel.Account
{
	public class LoggedOutViewModel
	{
		public string PostLogoutRedirectUri
		{
			get;
			set;
		}

		public string ClientName
		{
			get;
			set;
		}

		public string SignOutIframeUrl
		{
			get;
			set;
		}

		public bool AutomaticRedirectAfterSignOut
		{
			get;
			set;
		} = false;

		public string LogoutId
		{
			get;
			set;
		}

		public bool TriggerExternalSignout => ExternalAuthenticationScheme != null;

		public string ExternalAuthenticationScheme
		{
			get;
			set;
		}
	}
}