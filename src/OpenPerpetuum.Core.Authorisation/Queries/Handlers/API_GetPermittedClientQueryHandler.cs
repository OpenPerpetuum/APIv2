using OpenPerpetuum.Core.Authorisation.DatabaseResults;
using OpenPerpetuum.Core.Authorisation.Models;
using OpenPerpetuum.Core.DataServices.Database;
using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using OpenPerpetuum.Core.Foundation.Processing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace OpenPerpetuum.Core.Authorisation.Queries.Handlers
{
	internal class API_GetPermittedClientQueryHandler : IQueryHandler<API_GetPermittedClientQuery, ReadOnlyCollection<AccessClientModel>>
	{
		private readonly IDatabaseProvider dataProvider;

		public API_GetPermittedClientQueryHandler(IDataContext dataContext)
		{
			dataProvider = dataContext.GetDataContext("API");
		}

		public ReadOnlyCollection<AccessClientModel> Handle(API_GetPermittedClientQuery query)
		{
			IResult<AccessClientsResult> result = dataProvider.ExecuteProcedure<AccessClientsResult>(
				"Authorisation.GetAccessClients",
				new DbParameters
				{
					{ "ClientId", (object)query.ClientId ?? DBNull.Value }
				});

			result.ValidateResult();

			var clients = new List<AccessClientModel>();

			foreach (var client in result.Data.Clients)
				clients.Add(new AccessClientModel
				{
					AdministratorContactAddress = client.AdministratorContactAddress,
					AdministratorName = client.AdministratorName,
					ClientId = client.ClientId,
					FriendlyName = client.FriendlyName,
					RedirectUri = client.RedirectUri,
					IsAdministratorApp = client.IsAdministratorApp,
					Secret = client.Secret
				});

			return clients.AsReadOnly();
		}
	}
}
