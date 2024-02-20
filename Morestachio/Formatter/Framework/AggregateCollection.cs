namespace Morestachio.Formatter.Framework;

/// <summary>
///		Aggregates two Collections
/// </summary>
/// <typeparam name="T"></typeparam>
public class AggregateCollection<T> : ICollection<T>
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="self"></param>
	/// <param name="parent"></param>
	public AggregateCollection(ICollection<T> self, ICollection<T> parent)
	{
		_self = self;
		_parent = parent;
	}

	private readonly ICollection<T> _self;
	private readonly ICollection<T> _parent;

	private class AggregateEnumerator : IEnumerator<T>
	{
		private readonly IEnumerator<T> _self;
		private readonly IEnumerator<T> _parent;

		public AggregateEnumerator(IEnumerator<T> self, IEnumerator<T> parent)
		{
			_self = self;
			_parent = parent;
			_current = _self;
		}

		private IEnumerator<T> _current;

		/// <inheritdoc />
		public bool MoveNext()
		{
			var moveNext = _current.MoveNext();

			if (!moveNext)
			{
				if (_current == _parent)
				{
					return false;
				}

				_current = _parent;
				return _current.MoveNext();
			}

			return true;
		}

		/// <inheritdoc />
		public void Reset()
		{
			_current.Reset();
		}

		/// <inheritdoc />
		public T Current
		{
			get { return _current.Current; }
		}

		/// <inheritdoc />
		object IEnumerator.Current
		{
			get { return ((IEnumerator)_current).Current; }
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_self.Dispose();
			_parent.Dispose();
		}
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		return new AggregateEnumerator(_self.GetEnumerator(),
			_parent.GetEnumerator());
	}

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable)_self).GetEnumerator();
	}

	/// <inheritdoc />
	public void Add(T item)
	{
		_self.Add(item);
	}

	/// <inheritdoc />
	public void Clear()
	{
		_self.Clear();
	}

	/// <inheritdoc />
	public bool Contains(T item)
	{
		return _self.Contains(item) || _parent.Contains(item);
	}

	/// <inheritdoc />
	public void CopyTo(T[] array, int arrayIndex)
	{
		_self.CopyTo(array, arrayIndex);
		_parent.CopyTo(array, arrayIndex + _self.Count);
	}

	/// <inheritdoc />
	public bool Remove(T item)
	{
		return _self.Remove(item);
	}

	/// <inheritdoc />
	public int Count
	{
		get { return _self.Count + _parent.Count; }
	}

	/// <inheritdoc />
	public bool IsReadOnly
	{
		get { return _self.IsReadOnly; }
	}
}