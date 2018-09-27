using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OpenPerpetuum.Core.DataServices.MSSQL
{
	internal static class SqlDbTypeHelper
	{
		private static Dictionary<Type, SqlDbType> typeMap;

		static SqlDbTypeHelper()
		{
			typeMap = new Dictionary<Type, SqlDbType>
			{
				[typeof(Guid)] = SqlDbType.UniqueIdentifier,
				[typeof(string)] = SqlDbType.NVarChar,
				[typeof(char[])] = SqlDbType.NVarChar,
				[typeof(byte)] = SqlDbType.TinyInt,
				[typeof(short)] = SqlDbType.SmallInt,
				[typeof(int)] = SqlDbType.Int,
				[typeof(long)] = SqlDbType.BigInt,
				[typeof(byte[])] = SqlDbType.Image,
				[typeof(bool)] = SqlDbType.Bit,
				[typeof(DateTime)] = SqlDbType.DateTime2,
				[typeof(DateTimeOffset)] = SqlDbType.DateTimeOffset,
				[typeof(decimal)] = SqlDbType.Decimal,
				[typeof(float)] = SqlDbType.Real,
				[typeof(double)] = SqlDbType.Float,
				[typeof(TimeSpan)] = SqlDbType.Time
			};
		}

		// Non-generic argument-based method
		public static SqlDbType GetDbType(Type propertyType)
		{
			if (propertyType == null)
				throw new ArgumentNullException("giveType", "Type must be provided");
			// Allow nullable types to be handled
			propertyType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

			if (typeMap.ContainsKey(propertyType))
			{
				return typeMap[propertyType];
			}

			throw new ArgumentException($"{propertyType.FullName} is not a supported .NET class");
		}

		// Generic version
		public static SqlDbType GetDbType<TPropertyType>()
		{
			return GetDbType(typeof(TPropertyType));
		}
	}
}
