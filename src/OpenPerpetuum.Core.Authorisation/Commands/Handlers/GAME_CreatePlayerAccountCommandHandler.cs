using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.DataServices.Database.ResultTypes;
using OpenPerpetuum.Core.Foundation.Processing;
using OpenPerpetuum.Core.Foundation.Security;

namespace OpenPerpetuum.Core.Authorisation.Commands.Handlers
{
	internal class GAME_CreatePlayerAccountCommandHandler : ICommandHandler<GAME_CreatePlayerAccountCommand>
	{
		private readonly IDatabaseProvider gameDatabase;

		public GAME_CreatePlayerAccountCommandHandler(IDataContext dataContext)
		{
			gameDatabase = dataContext.GetDataContext("Game");
		}

		public void Handle(GAME_CreatePlayerAccountCommand command)
		{
			IResult<NoResult> dbCommand = gameDatabase.ExecuteProcedure<NoResult>(
				"API.CreatePlayerAccount",
				new DbParameters
				{
					{ "Email", command.EmailAddress },
					{ "HashedPassword", command.Password.ToLegacyShaString() },
					{ "FirstName", command.FirstName },
					{ "LastName", command.LastName },
					{ "DateOfBirth", command.DateOfBirth.Date },
					{ "CreationDate", command.CreationDate.Date }
				});

			dbCommand.ValidateResult();
		}
	}
}
