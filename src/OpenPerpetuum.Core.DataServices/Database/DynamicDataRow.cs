using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace OpenPerpetuum.Core.DataServices.Database
{
	public abstract class DynamicDataRow
	{
		protected Type ClassType;
		protected readonly Dictionary<string, PropertyInfo> Properties;
		protected readonly Dictionary<string, DataItemAttribute> Attributes;

		public ReadOnlyCollection<string> PropertyNames => Properties.Keys.ToList().AsReadOnly();

		public DynamicDataRow()
		{
			ClassType = GetType();
			Properties = ClassType
				.GetProperties()
				.Where(p => GetAttributes(p) != null)
				.ToDictionary(p => p.Name, p => p);

			Attributes = Properties
				.Where(p => GetAttributes(p.Value) != null)
				.ToDictionary(p => p.Value.Name, p => GetAttributes(p.Value));
		}

		public object this[string propertyName]
		{
			get
			{
				if (Properties.ContainsKey(propertyName))
				{
					var value = Properties[propertyName].GetValue(this, null);
					return value;
				}

				return null;
			}
		}

		protected DataItemAttribute GetAttributes(PropertyInfo pi)
		{
			return pi.GetCustomAttributes<DataItemAttribute>().SingleOrDefault();
		}

		internal Type GetPropertyType(string propertyName)
		{
			if (Properties.ContainsKey(propertyName))
				return Properties[propertyName].PropertyType;
			else
				return null;
		}

		internal DataItemAttribute GetPropertyAttribute(string propertyName)
		{
			return Attributes[propertyName];
		}
	}
}
