using System;
using System.Collections;
using System.Collections.Generic;
using Morestachio.Framework.Expression;

namespace Morestachio.Framework
{
	/// <summary>
	///		Creates a Traversable collection that keeps track of the current element while still providing the whole list
	/// </summary>
	public class Traversable : IEnumerator<KeyValuePair<string, PathType>>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="parts"></param>
		public Traversable(IEnumerable<KeyValuePair<string, PathType>> parts)
		{
			_list = new LinkedList<KeyValuePair<string, PathType>>(parts);
			_node = _list.First;
			_current = default;
			_index = 0;
		}

		private readonly LinkedList<KeyValuePair<string, PathType>> _list;
		private LinkedListNode<KeyValuePair<string, PathType>> _node;
		private KeyValuePair<string, PathType> _current;
		private int _index;

		public IEnumerable<KeyValuePair<string, PathType>> List
		{
			get
			{
				return _list;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int Count
		{
			get
			{
				return _list.Count;
			}
		}

		/// <summary>
		///		Gets the next element
		/// </summary>
		/// <returns></returns>
		public KeyValuePair<string, PathType> Peek()
		{
			return _node?.Value ?? default;
		}

		/// <summary>
		///		Does the traversable have any elements
		/// </summary>
		/// <returns></returns>
		public bool Any()
		{
			return _node != null;
		}

		/// <summary>
		///		Gets the next element and returns it
		/// </summary>
		/// <returns></returns>
		public KeyValuePair<string, PathType> Dequeue()
		{
			MoveNext();
			return Current;
		}

		/// <summary>
		///		Gets the next element
		/// </summary>
		/// <returns></returns>
		public bool MoveNext()
		{
			if (_node == null)
			{
				_index = _list.Count + 1;
				return false;
			}

			++_index;
			_current = _node.Value;
			_node = _node.Next;
			if (_node == _list.First)
			{
				_node = null;
			}
			return true;
		}

		/// <summary>
		///		Resets the traversable
		/// </summary>
		public void Reset()
		{
			_current = default;
			_node = _list.First;
			_index = 0;
		}

		public KeyValuePair<string, PathType> Current
		{
			get { return _current; }
		}

		object IEnumerator.Current
		{
			get
			{
				if (_index == 0 || (_index == _list.Count + 1))
				{
					throw new InvalidOperationException();
				}

				return Current;
			}
		}

		public void Dispose()
		{

		}
	}
}