using OpenPerpetuum.IdentityServer.InputModel.Account;

namespace OpenPerpetuum.IdentityServer.ViewModel.Account
{
	public class LogoutViewModel : LogoutInputModel
	{
		public bool ShowLogoutPrompt
		{
			get;
			set;
		} = true;
	}
}
