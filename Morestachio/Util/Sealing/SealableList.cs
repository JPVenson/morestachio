using System;
using System.Collections;
using System.Collections.Generic;

namespace Morestachio.Util.Sealing
{
	internal class SealableList<T> : IList<T>, ISealed
	{
		internal SealableList()
		{
			_base = new List<T>();
		}

		private readonly IList<T> _base;

		public bool IsSealed { get; private set; }

		protected void CheckSealed()
		{
			if (IsSealed)
			{
				throw new InvalidOperationException("This instance of ParserOptions is sealed and cannot be modified anymore");
			}
		}

		public virtual void Seal()
		{
			IsSealed = true;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable)_base).GetEnumerator();
		}

		public void Add(T item)
		{
			CheckSealed();
			_base.Add(item);
		}

		public void Clear()
		{
			CheckSealed();
			_base.Clear();
		}

		public bool Contains(T item)
		{
			return _base.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			_base.CopyTo(array, arrayIndex);
		}

		public bool Remove(T item)
		{
			CheckSealed();
			return _base.Remove(item);
		}

		public int Count
		{
			get { return _base.Count; }
		}

		public bool IsReadOnly
		{
			get { return _base.IsReadOnly; }
		}

		public int IndexOf(T item)
		{
			return _base.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			CheckSealed();
			_base.Insert(index, item);
		}

		public void RemoveAt(int index)
		{
			CheckSealed();
			_base.RemoveAt(index);
		}

		public T this[int index]
		{
			get { return _base[index]; }
			set
			{
				CheckSealed();
				_base[index] = value;
			}
		}
	}
}