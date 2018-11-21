using System;
using System.Collections.Generic;

namespace OpenPerpetuum.IdentityServer.ViewModel.Grants
{
	public class GrantViewModel
	{
		public string ClientId
		{
			get;
			set;
		}

		public string ClientName
		{
			get;
			set;
		}

		public string ClientUrl
		{
			get;
			set;
		}

		public string ClientLogoUrl
		{
			get;
			set;
		}

		public DateTimeOffset Created
		{
			get;
			set;
		}

		public DateTimeOffset? Expires
		{
			get;
			set;
		}

		public IEnumerable<string> IdentityGrantNames
		{
			get;
			set;
		}

		public IEnumerable<string> ApiGrantNames
		{
			get;
			set;
		}
	}
}