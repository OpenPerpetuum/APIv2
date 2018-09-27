using System;
using System.Collections.Generic;
using System.Text;

namespace OpenPerpetuum.Core.DataServices.Interfaces
{
	public interface IDataContext
	{
		IDatabaseProvider GetDataContext(string contextName);
		IDatabaseProvider GetDataContext<TContext>();
	}
}
