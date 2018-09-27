using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Dynamic;

namespace OpenPerpetuum.Core.Extensions
{
	public static class DynamicHelper
	{
		public static bool PropertyExists(dynamic dynamicObject, string propertyName)
		{
			if (dynamicObject is JObject)
				return ((IDictionary<string, JToken>)dynamicObject).ContainsKey(propertyName);

			if (dynamicObject is ExpandoObject)
				return ((IDictionary<string, object>)dynamicObject).ContainsKey(propertyName);

			return dynamicObject.GetType().GetProperty(propertyName) != null;
		}
	}
}
