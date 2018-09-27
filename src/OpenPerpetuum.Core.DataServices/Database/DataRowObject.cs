using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace OpenPerpetuum.Core.DataServices.Database
{
	public class DataRowObject : NameObjectCollectionBase
	{
		public void Add(string key, object value)
		{
			BaseAdd(key, value);
		}

		public KeyValuePair<string, object> this[int index]
		{
			get { return new KeyValuePair<string, object>(BaseGetKey(index), BaseGet(index)); }
		}

		public object this[string key]
		{
			get
			{
				object theValue = BaseGet(key);
				if (theValue == DBNull.Value)
					return null;
				else
					return theValue;
			}
		}

		public override IEnumerator GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
				yield return this[i];
		}
	}
}
