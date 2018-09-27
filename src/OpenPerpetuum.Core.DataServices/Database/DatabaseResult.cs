using System.Collections.Generic;

namespace OpenPerpetuum.Core.DataServices.Database
{
	public abstract class DatabaseResult
	{
		public SortedList<int, ResultSet> Results
		{
			get; private set;
		}

		public int ReturnValue
		{
			get; set;
		}

		public DatabaseResult()
		{
			Results = new SortedList<int, ResultSet>();
		}
	}
}
