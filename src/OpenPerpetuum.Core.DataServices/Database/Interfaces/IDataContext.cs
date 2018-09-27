namespace OpenPerpetuum.Core.DataServices.Database.Interfaces
{
	public interface IDataContext
	{
		void AddDataContext(IDatabaseProvider databaseProvider);
		void RemoveDataContext(IDatabaseProvider databaseProvider);
		IDatabaseProvider GetDataContext(string contextName);
		IDatabaseProvider GetDataContext<TContext>();
	}
}
