using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.Foundation.Processing;

namespace OpenPerpetuum.Core.Authorisation.Queries
{
	public class GAME_AuthenticateAndGetUserDetailsQuery : IQuery<UserModel>
	{
		public string Email
		{
			get;
			set;
		}

		public string EncryptedPassword
		{
			get;
			set;
		}
	}
}
