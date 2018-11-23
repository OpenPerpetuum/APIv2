using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.ComponentModel.DataAnnotations;

namespace OpenPerpetuum.Core.Authorisation.Commands
{
	public class GAME_CreatePlayerAccountCommand : ICommand
	{
		[StringLength(50)]
		public string EmailAddress
		{
			get;
			set;
		}

		public string Password
		{
			get;
			set;
		}

		[StringLength(50)]
		public string FirstName
		{
			get;
			set;
		}

		[StringLength(50)]
		public string LastName
		{
			get;
			set;
		}

		public DateTime DateOfBirth
		{
			get;
			set;
		}

		public DateTime CreationDate
		{
			get;
			set;
		}
	}
}
