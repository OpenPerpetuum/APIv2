using OpenPerpetuum.IdentityServer.InputModel.Consent;
using System.Collections.Generic;

namespace OpenPerpetuum.IdentityServer.ViewModel.Consent
{
	public class ConsentViewModel : ConsentInputModel
	{
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

		public bool AllowRememberConsent
		{
			get;
			set;
		}

		public IEnumerable<ScopeViewModel> IdentityScopes
		{
			get;
			set;
		}

		public IEnumerable<ScopeViewModel> ResourceScopes
		{
			get;
			set;
		}
	}
}
