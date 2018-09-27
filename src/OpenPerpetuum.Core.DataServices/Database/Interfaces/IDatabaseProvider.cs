using System.Collections.Generic;

namespace OpenPerpetuum.Core.DataServices.Database.Interfaces
{
	public interface IDatabaseProvider
	{
		string ConnectionString { get; }
		IResult<TResultObject> ExecuteProcedure<TResultObject>(string procedureName, IDictionary<string, object> parameters)
			where TResultObject : DatabaseResult, new();
	}
}
