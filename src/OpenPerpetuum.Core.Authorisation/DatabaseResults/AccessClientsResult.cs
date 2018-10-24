using OpenPerpetuum.Core.DataServices.Database;
using System;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Authorisation.DatabaseResults
{
	#region Database class data
	internal class AccessClientData
	{
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
	}
	#endregion
	
	internal class AccessClientsResult : DatabaseResult
	{
		public ReadOnlyCollection<AccessClientData> Clients => ((ResultSet<AccessClientData>)Results[0])?.Data;

		public AccessClientsResult()
		{
			Results.Add(0, new ResultSet<AccessClientData>(0));
		}
	}
}
