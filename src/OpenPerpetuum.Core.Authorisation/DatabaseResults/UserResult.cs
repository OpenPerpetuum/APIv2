using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.DataServices.Database;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Authorisation.DatabaseResults
{
	internal class UserData
	{
		public int AccountId
		{
			get;
			set;
		}

		public string Email
		{
			get;
			set;
		}

		public string FirstName
		{
			get;
			set;
		}

		public string LastName
		{
			get;
			set;
		}

		public AccessLevel AccessLevel
		{
			get;
			set;
		}

		public DateTime LastLoggedIn
		{
			get;
			set;
		}

		public DateTime CreationDate
		{
			get;
			set;
		}

		public DateTime BanDate
		{
			get;
			set;
		}

		public int BanLengthMinutes
		{
			get;
			set;
		}

		public bool EmailConfirmed
		{
			get;
			set;
		}

		public bool IsActive
		{
			get;
			set;
		}

		public int TotalMinutesOnline
		{
			get;
			set;
		}
	}

	internal class UserResult : DatabaseResult
	{
		public ReadOnlyCollection<UserData> Users => ((ResultSet<UserData>)Results[0])?.Data ??  new List<UserData>().AsReadOnly();

		public UserResult()
		{
			Results.Add(0, new ResultSet<UserData>(0));
		}
	}
}
