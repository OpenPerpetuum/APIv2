using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OpenPerpetuum.Core.DataServices
{
	/// <summary>
	/// Used to pass parameters to database stored procedures in a recognisable and concise way
	/// </summary>
	public class DbParameters : IDictionary<string, object>
	{
		public static DbParameters None => new DbParameters();
		private readonly Dictionary<string, object> collection = new Dictionary<string, object>();

		public object this[string key]
		{
			get => collection[key];
			set => collection[key] = value;
		}

		public ICollection<string> Keys => collection.Keys;
		public ICollection<object> Values => collection.Values;

		public int Count => collection.Count;
		public bool IsReadOnly => false;

		public void Add(string key, object value)
		{
			collection.Add(key, value);
		}

		public void Add(KeyValuePair<string, object> item)
		{
			collection.Add(item.Key, item.Value);
		}

		public void Clear()
		{
			collection.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			return collection.Contains(item);
		}

		public bool ContainsKey(string key)
		{
			return collection.ContainsKey(key);
		}

		public bool ContainsValue(object value)
		{
			return collection.ContainsValue(value);
		}

		/// <summary>
		/// Copy the current collection to a new array
		/// </summary>
		/// <param name="target">Target array to copy to</param>
		/// <param name="startIndex">The 0 based index where the copy should begin</param>
		public void CopyTo(KeyValuePair<string, object>[] target, int startIndex)
		{
			((IDictionary<string, object>)collection).CopyTo(target, startIndex);
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return collection.GetEnumerator();
		}

		public bool Remove(string key)
		{
			return collection.Remove(key);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			if (collection.ContainsKey(item.Key))
				return collection.Remove(item.Key);

			return false;
		}

		public bool TryGetValue(string key, out object value)
		{
			bool result = collection.TryGetValue(key, out value);

			return result;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)collection).GetEnumerator();
		}
	}
}
