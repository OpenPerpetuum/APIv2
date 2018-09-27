using System.Collections;
using System.Collections.Generic;

namespace OpenPerpetuum.Core.DataServices.Database
{
	public class DynamicDataTable : List<DynamicDataRow>
	{
		public string DatabaseTypeName
		{
			get;
			protected set;
		}
	}

	public class DynamicDataTable<TTableType> : DynamicDataTable, IEnumerable
		where TTableType : DynamicDataRow
	{
		public DynamicDataTable(string databaseTypeName)
		{
			DatabaseTypeName = databaseTypeName;
		}

		public new TTableType this[int index]
		{
			get
			{
				return base[index] as TTableType;
			}
		}
	}
}
