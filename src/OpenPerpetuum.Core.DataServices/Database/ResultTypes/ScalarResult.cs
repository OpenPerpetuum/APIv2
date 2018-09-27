using System;
using System.Linq;

namespace OpenPerpetuum.Core.DataServices.Database.ResultTypes
{
	/// <summary>
	/// Used to retrieve a single scalar figure from a query. All results are treat as nullable, validate accordingly.
	/// </summary>
	/// <typeparam name="TPrimitive"></typeparam>
	public sealed class ScalarResult<TPrimitive> : DatabaseResult
		where TPrimitive : struct
	{
		public TPrimitive? ScalarValue
		{
			get
			{
				object value = ((ResultSet<TPrimitive>)Results[0])?.Data?.SingleOrDefault();
				if (!(value is DBNull))
					return (TPrimitive)value;
				else return null;
			}
		}

		public ScalarResult()
		{
			Results.Add(0, new ResultSet<TPrimitive>(0));
		}
	}
}
