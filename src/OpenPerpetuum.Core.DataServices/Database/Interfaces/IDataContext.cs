namespace OpenPerpetuum.Core.DataServices.Database.Interfaces
{
	public interface IDataContext
	{
		IDatabaseProvider GetDataContext(string contextName);
		IDatabaseProvider GetDataContext<TContext>();
	}
}
