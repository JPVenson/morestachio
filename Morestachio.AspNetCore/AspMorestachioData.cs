using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Morestachio.AspNetCore
{
	public class AspMorestachioData : IDictionary<string, object>
	{
		public AspMorestachioData()
		{
			_extValues = new Dictionary<string, object>();
		}

		private IDictionary<string, object> _extValues;

		public object Data
		{
			get { return _extValues[nameof(Data)]; }
			set { _extValues[nameof(Data)] = value; }
		}

		public HttpContext Context
		{
			get { return _extValues[nameof(Context)] as HttpContext; }
			set { _extValues[nameof(Context)] = value; }
		}

		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return _extValues.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_extValues).GetEnumerator();
		}

		public void Add(KeyValuePair<string, object> item)
		{
			_extValues.Add(item);
		}

		public void Clear()
		{
			_extValues.Clear();
		}

		public bool Contains(KeyValuePair<string, object> item)
		{
			return _extValues.Contains(item);
		}

		public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
		{
			_extValues.CopyTo(array, arrayIndex);
		}

		public bool Remove(KeyValuePair<string, object> item)
		{
			return _extValues.Remove(item);
		}

		public int Count
		{
			get { return _extValues.Count; }
		}

		public bool IsReadOnly
		{
			get { return _extValues.IsReadOnly; }
		}

		public void Add(string key, object value)
		{
			_extValues.Add(key, value);
		}

		public bool ContainsKey(string key)
		{
			return _extValues.ContainsKey(key);
		}

		public bool Remove(string key)
		{
			return _extValues.Remove(key);
		}

		public bool TryGetValue(string key, out object value)
		{
			return _extValues.TryGetValue(key, out value);
		}

		public object this[string key]
		{
			get { return _extValues[key]; }
			set { _extValues[key] = value; }
		}

		public ICollection<string> Keys
		{
			get { return _extValues.Keys; }
		}

		public ICollection<object> Values
		{
			get { return _extValues.Values; }
		}
	}
}