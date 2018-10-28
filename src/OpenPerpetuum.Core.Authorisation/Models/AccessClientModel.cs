using System;
using System.ComponentModel.DataAnnotations;

namespace OpenPerpetuum.Core.Authorisation.Models
{
	public class AccessClientModel
	{
		public static AccessClientModel DefaultValue
		{
			get
			{
				return new AccessClientModel
				{
					AdministratorContactAddress = "default@value",
					AdministratorName = "Default Value",
					ClientId = Guid.Empty,
					FriendlyName = "Default Value",
					RedirectUri = "http://www.open-perpetuum.com",
					Secret = string.Empty,
					IsAdministratorApp = false
				};
			}
		}

		[Key]
		public Guid ClientId
		{
			get;
			set;
		}

		public string RedirectUri
		{
			get;
			set;
		}

		public string FriendlyName
		{
			get;
			set;
		}

		public string AdministratorName
		{
			get;
			set;
		}

		public string AdministratorContactAddress
		{
			get;
			set;
		}

		public string Secret
		{
			get;
			set;
		}

		public bool IsAdministratorApp
		{
			get;
			set;
		}
	}
}
