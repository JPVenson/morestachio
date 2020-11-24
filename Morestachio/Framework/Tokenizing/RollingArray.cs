using System;
using System.Collections;
using System.Collections.Generic;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///		Creates a new fixed length ring buffer
	/// </summary>
	public class RollingArray<T> : IEnumerable<T>
	{
		/// <summary>
		///		
		/// </summary>
		/// <param name="size"></param>
		public RollingArray(int size)
		{
			_buffer = new T[size];
			_writerIndex = -1;
		}

		private int _writerIndex;
		private readonly T[] _buffer;

		/// <summary>
		///		Gets the length of the <see cref="RollingArray{T}"/>
		/// </summary>
		public int Length
		{
			get
			{
				return _buffer.Length;
			}
		}

		/// <summary>
		///		Gets the current writer position within the bounds of <see cref="_buffer"/>
		/// </summary>
		/// <returns></returns>
		internal int Pos()
		{
			return _writerIndex % _buffer.Length;
		}

		/// <summary>
		///		Gets the char at the natural index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get
			{
				return _buffer[Translate(index)];
			}
		}

		/// <summary>
		///		Translates a natural index 0-Length to a real index depending on the current writer index
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		internal int Translate(int index)
		{
			var inx = (Pos() + 1) + index;
			if (inx >= _buffer.Length)
			{
				//in this case we underflow the array length so set it back to the end of the array
				return inx - _buffer.Length;
			}

			return inx;
		}

		/// <summary>
		///		Returns the contents of the array
		/// </summary>
		/// <returns></returns>
		public T[] ToArray()
		{
			T[] arr;

			if (_writerIndex < _buffer.Length)
			{
				//if there are less written elements than the size of the buffer, omit those
				arr = new T[_writerIndex + 1];
				for (int i = 0; i < _writerIndex + 1; i++)
				{
					arr[i] = _buffer[i];
				}
			}
			else
			{
				arr = new T[_buffer.Length];
				for (int i = 0; i < _buffer.Length; i++)
				{
					arr[i] = this[i];
				}
			}


			return arr;
		}

		/// <summary>
		///		Adds a new element to the list
		/// </summary>
		/// <param name="c"></param>
		public void Add(T c)
		{
			_writerIndex += 1;
			_buffer[Pos()] = c;
		}

		/// <summary>
		///		Checks if the elements are equal to the elements at the start of the array
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		public bool StartsWith(T[] elements, IEqualityComparer<T> comparer = null)
		{
			if (elements.Length > _buffer.Length)
			{
				throw new IndexOutOfRangeException("The number of elements exceeds the size of the array");
			}

			for (int i = 0; i < elements.Length; i++)
			{
				var objA = elements[i];
				var objB = this[i];
				if (!comparer?.Equals(objA, objB) ?? !Equals(objA, objB))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///		Checks if the elements are equal to the elements at the end of the array
		/// </summary>
		/// <param name="elements"></param>
		/// <returns></returns>
		public bool EndsWith(T[] elements, IEqualityComparer<T> comparer = null)
		{
			if (elements.Length > _buffer.Length)
			{
				throw new IndexOutOfRangeException("The number of elements exceeds the size of the array");
			}
			for (int i = 0; i < elements.Length; i++)
			{
				var objA = elements[elements.Length - (i + 1)];
				var objB = this[_buffer.Length - (i + 1)];
				if (!comparer?.Equals(objA, objB) ?? !Equals(objA, objB))
				{
					return false;
				}
			}

			return true;
		}

		private class RollingArrayEnumerator : IEnumerator<T>
		{
			private readonly RollingArray<T> _array;
			private readonly int _startIndex;
			private int _index;

			public RollingArrayEnumerator(RollingArray<T> array)
			{
				_array = array;
				_startIndex = _array._writerIndex;
				_index = 0;
			}

			public bool MoveNext()
			{
				if (_startIndex != _array._writerIndex)
				{
					throw new InvalidOperationException($"The {nameof(RollingArray<T>)} has been modified");
				}

				_index++;
				if (_index > _array.Length)
				{
					return false;
				}

				Current = _array[_index];
				return true;
			}

			public void Reset()
			{
				_index = 0;
			}

			public T Current { get; private set; }

			object IEnumerator.Current
			{
				get { return Current; }
			}

			public void Dispose()
			{
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return new RollingArrayEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}