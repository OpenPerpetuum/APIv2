using System.ComponentModel.DataAnnotations;

namespace OpenPerpetuum.Api.Models.Authorisation
{
	public class LoginViewModel
	{
		public string Username
		{
			get;
			set;
		}

		[DataType(DataType.Password)]
		public string Password
		{
			get;
			set;
		}

		public string ReturnUrl
		{
			get;
			set;
		}
	}
}
