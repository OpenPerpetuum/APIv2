using OpenPerpetuum.IdentityServer.Configuration;
using OpenPerpetuum.IdentityServer.InputModel.Account;
using System.Collections.Generic;
using System.Linq;

namespace OpenPerpetuum.IdentityServer.ViewModel.Account
{
	public class LoginViewModel : LoginInputModel
	{
		public bool AllowRememberLogin
		{
			get;
			set;
		}

		public bool EnableLocalLogin
		{
			get;
			set;
		}

		public IEnumerable<ExternalProvider> ExternalProviders
		{
			get;
			set;
		}

		public IEnumerable<ExternalProvider> VisibleExternalProviders => ExternalProviders.Where(ep => !string.IsNullOrWhiteSpace(ep.DisplayName));

		public bool IsExternalLoginOnly => EnableLocalLogin == false && ExternalProviders?.Count() == 1;
		public string ExternalLoginScheme => IsExternalLoginOnly ? ExternalProviders?.SingleOrDefault()?.AuthenticationScheme : null;
	}
}
