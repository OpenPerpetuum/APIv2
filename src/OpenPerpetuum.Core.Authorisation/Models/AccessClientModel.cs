using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
					RedirectUris = new List<Uri> { new Uri("http://www.open-perpetuum.com") }.AsReadOnly()
				};
			}
		}
		public Guid ClientId
		{
			get;
			set;
		}

		public ReadOnlyCollection<Uri> RedirectUris
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
	}
}
