using OpenPerpetuum.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.Core.DataServices.Database
{
	public abstract class ResultSet
	{
		public ResultSet(int index)
		{
			Index = index;
		}

		public virtual int Index
		{
			get;
			protected set;
		}

		public static ResultSet<TResultObject> CreateResultSet<TResultObject>(int resultSetIndex, IDataReader reader)
		{
			var dataRows = new SortedList<int, DataRowObject>();
			int rowCounter = 0;
			while(reader.Read())
			{
				var dataRow = new DataRowObject();
				for (int i = 0; i < reader.FieldCount; i++)
					dataRow.Add(reader.GetName(i), reader[i]);

				dataRows.Add(rowCounter++, dataRow);
			}

			ResultSet<TResultObject> resultSet = new ResultSet<TResultObject>(resultSetIndex);

			foreach (var dataRow in dataRows)
				resultSet.AddData(dataRow.Key, dataRow.Value);

			return resultSet;
		}
	}

	public class ResultSet<TResultObject> : ResultSet
	{
		private List<TResultObject> dataCollection = new List<TResultObject>();

		public ResultSet(int index)
			: base(index)
		{ }

		public ReadOnlyCollection<TResultObject> Data => dataCollection.AsReadOnly();

		public void AddData(int rowNumber, DataRowObject dataRow)
		{
			TResultObject resultObject;
			TypeInfo resultObjectInfo = typeof(TResultObject).GetTypeInfo();
			if (resultObjectInfo.IsClass && resultObjectInfo.IsArray == false && typeof(TResultObject) != typeof(string))
			{
				var method = typeof(ResultSet<TResultObject>).GetMethod("ParseObjectResult", BindingFlags.Instance | BindingFlags.NonPublic);
				var genericMethod = method.MakeGenericMethod(typeof(TResultObject));
				resultObject = (TResultObject)genericMethod.Invoke(this, new object[] { dataRow });
			}
			else
				resultObject = ParseScalarResult(dataRow[0].Value);

			dataCollection.Add(resultObject);
		}

		private TResultObject ParseScalarResult(object value)
		{
			TResultObject resultObject = (TResultObject)Convert.ChangeType(value, typeof(TResultObject));

			return resultObject;
		}

		private TResultObject ParseObjectResult<TDataObject>(DataRowObject dataRow)
			where TDataObject : class, TResultObject, new()
		{
			Type resultObjectType = typeof(TDataObject);
			TDataObject resultObject = new TDataObject();

			foreach (var field in dataRow.Keys.Cast<string>())
			{
				var property = resultObjectType.GetProperty(field);
				if (property != null)
				{
					object valueToSet = dataRow[field];
					if ((property.PropertyType.IsEnum || EnumExtensions.IsNullableEnum(property.PropertyType)) && valueToSet is string)
					{
						if (EnumExtensions.IsNullableEnum(property.PropertyType))
							valueToSet = Enum.Parse(Nullable.GetUnderlyingType(property.PropertyType), (string)valueToSet);
						else
							valueToSet = Enum.Parse(property.PropertyType, (string)valueToSet);
					}

					property.SetValue(resultObject, valueToSet);
				}
			}

			return resultObject;
		}
	}
}
