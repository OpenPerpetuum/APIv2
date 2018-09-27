using OpenPerpetuum.Core.DataServices.Database.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenPerpetuum.Core.DataServices.Context
{
	public class DataContext : IDataContext
	{
		private readonly Dictionary<string, IDatabaseProvider> databaseProviders = new Dictionary<string, IDatabaseProvider>();

		public void AddDataContext(IDatabaseProvider databaseProvider)
		{
			if (databaseProviders.ContainsKey(databaseProvider.ProviderName))
				throw new InvalidOperationException($"There is already a database provider registered for \"{databaseProvider.ProviderName}\"");
			databaseProviders.Add(databaseProvider.ProviderName, databaseProvider);
		}

		public IDatabaseProvider GetDataContext(string contextName)
		{
			if (!databaseProviders.ContainsKey(contextName))
				throw new KeyNotFoundException($"No database provider registered for \"{contextName}\"");

			return databaseProviders[contextName];
		}

		public IDatabaseProvider GetDataContext<TContext>()
		{
			var provider = databaseProviders.Values.Where(dp => dp.GetType() == typeof(TContext)).ToList();
			
			if (provider.Count > 1)
				throw new InvalidOperationException($"Unable to find distinct datacontext. Use the name instead");

			if (provider.Count == 0)
				throw new KeyNotFoundException($"Unable to find an instance of the requested type. Use the name instead");

			return provider.Single();
		}

		public void RemoveDataContext(IDatabaseProvider databaseProvider)
		{
			databaseProviders.Remove(databaseProvider.ProviderName);
		}
	}
}
