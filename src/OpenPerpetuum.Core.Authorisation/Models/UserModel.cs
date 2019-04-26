using OpenPerpetuum.Core.SharedIdentity.Authorisation;
using System;

namespace OpenPerpetuum.Core.Authorisation.Models
{
	public class UserModel
	{
		public static UserModel Default = new UserModel
		{
			AccessLevel = AccessLevel.NotDefined,
			AccountId = -1,
			BanDate = DateTimeOffset.MinValue,
			BanExpires = DateTimeOffset.MaxValue,
			BanLength = DateTimeOffset.MaxValue.Subtract(DateTimeOffset.MinValue),
			CreationDate = DateTimeOffset.MinValue,
			Email = string.Empty,
			EmailConfirmed = false,
			FirstName = string.Empty,
			IsActive = false,
			LastLoggedIn = DateTimeOffset.MinValue,
			LastName = string.Empty,
			TotalTimeOnline = TimeSpan.FromTicks(0)
		};

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

		public DateTimeOffset LastLoggedIn
		{
			get;
			set;
		}

		public DateTimeOffset CreationDate
		{
			get;
			set;
		}

		public DateTimeOffset BanDate
		{
			get;
			set;
		}

		public DateTimeOffset BanExpires
		{
			get;
			set;
		}

		public TimeSpan BanLength
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

		public TimeSpan TotalTimeOnline
		{
			get;
			set;
		}

        public Permission[] Permissions
        {
            get
            {
                if (AccessLevel == AccessLevel.Admin)
                    return new Permission[] { Permission.OWNER };
                else
                    return new Permission[] { };
            }
        }
	}
}
