using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework.Expression.Framework;

namespace Morestachio.Framework
{
	/// <summary>
	///		Creates a Traversable collection that keeps track of the current element while still providing the whole list
	/// </summary>
	public class Traversable //: IEnumerator<KeyValuePair<string, PathType>>
	{
		/// <summary>
		///		An empty Traversable
		/// </summary>
		public static Traversable Empty { get; } = new Traversable(Enumerable.Empty<KeyValuePair<string, PathType>>());

		/// <summary>
		/// 
		/// </summary>
		public Traversable(IEnumerable<KeyValuePair<string, PathType>> parts)
		{
			var node = this;
			var values = parts.ToArray();
			if (values.Length == 0)
			{
				return;
			}

			node.Current = values[0];
			node.Count = values.Length - 1;

			for (var index = 1; index < values.Length; index++)
			{
				var keyValuePair = values[index];
				node._next = new Traversable(keyValuePair, values.Length - index - 1);
				node = node._next;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private Traversable(KeyValuePair<string, PathType> value, int count)
		{
			Current = value;
			Count = count;
		}

		///// <summary>
		///// 
		///// </summary>
		//public Traversable(string serialisedText) : this(serialisedText.Split(',').Select(f =>
		//{
		//	var parts = f.Trim('{', '}').Split(';');
		//	return new KeyValuePair<string, PathType>(parts.ElementAtOrDefault(1),
		//		(PathType)Enum.Parse(typeof(PathType), parts[0]));
		//}))
		//{
		//}

		//public string Serialize()
		//{
		//	return string.Join(",", ToArray().Select(f =>
		//	{
		//		if (f.Key != null)
		//		{
		//			return "{" + f.Value + ";" + f.Key + "}";
		//		}

		//		return "{" + f.Value + "}";
		//	}));
		//}

		private Traversable _next;

		/// <summary>
		///		Enumerates the <see cref="Traversable"/>
		/// </summary>
		/// <returns></returns>
		public KeyValuePair<string, PathType>[] ToArray()
		{
			if (!HasValue)
			{
				return Array.Empty<KeyValuePair<string, PathType>>();
			}

			var list = new KeyValuePair<string, PathType>[Count + 1];
			var node = this;
			for (int i = 0; i < list.Length; i++)
			{
				list[i] = node.Current;
				node = node._next;
			}

			return list;
		}

		/// <summary>
		/// 
		/// </summary>
		public int Count
		{
			get;
			private set;
		}

		/// <summary>
		///		Gets the next element
		/// </summary>
		/// <returns></returns>
		public KeyValuePair<string, PathType> Peek()
		{
			return _next?.Current ?? default;
		}

		/// <summary>
		///		Does the traversable have any elements
		/// </summary>
		/// <returns></returns>
		public bool Any()
		{
			return HasValue || _next != null;
		}

		/// <summary>
		///		Gets the next element and returns it
		/// </summary>
		/// <returns></returns>
		public Traversable Dequeue()
		{
			return _next;
		}

		/// <summary>
		///		Gets the current node value
		/// </summary>
		public KeyValuePair<string, PathType> Current
		{
			get;
			private set;
		}

		/// <summary>
		///		Gets if the current node has a value
		/// </summary>
		public bool HasValue
		{
			get
			{
				return !Equals(Current, default(KeyValuePair<string, PathType>));
			}
		}

		/// <summary>
		///		Expands the current node by creating a new Traversable and attaching the parameter to it
		/// </summary>
		/// <param name="getList"></param>
		/// <returns></returns>
		public Traversable Expand(IEnumerable<KeyValuePair<string, PathType>> getList)
		{
			return new Traversable(ToArray().Concat(getList));
		}
	}
}