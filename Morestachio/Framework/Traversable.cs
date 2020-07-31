using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework.Expression;
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
		/// <param name="parts"></param>
		public Traversable(IEnumerable<KeyValuePair<string, PathType>> parts)
		{
			Load(parts);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="parts"></param>
		public Traversable(string serialisedText)
		{
			Load(serialisedText.Split(',').Select(f =>
			{
				var parts = f.Trim('{', '}').Split(';');
				return new KeyValuePair<string, PathType>(parts.ElementAtOrDefault(1),
					(PathType)Enum.Parse(typeof(PathType), parts[0]));
			}));
		}

		private void Load(IEnumerable<KeyValuePair<string, PathType>> parts)
		{
			var node = this;
			var values = parts.ToArray();
			for (var index = 0; index < values.Length; index++)
			{
				var keyValuePair = values[index];
				node.Count = values.Length - index - 1;
				node.Current = keyValuePair;
				if (index + 1 < values.Length)
				{
					node._next = new Traversable(keyValuePair);
					node = node._next;
				}
			}
		}

		public string Serialize()
		{
			return string.Join(",", ToArray().Select(f =>
			{
				if (f.Key != null)
				{
					return "{" + f.Value + ";" + f.Key + "}";
				}

				return "{" + f.Value + "}";
			}));
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parts"></param>
		private Traversable(KeyValuePair<string, PathType> value)
		{
			Current = value;
		}

		private Traversable _next;

		public KeyValuePair<string, PathType>[] ToArray()
		{
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
		
		public KeyValuePair<string, PathType> Current
		{
			get;
			private set;
		}

		public bool HasValue
		{
			get
			{
				return !Equals(Current, default(KeyValuePair<string, PathType>));
			}
		}
	}
}