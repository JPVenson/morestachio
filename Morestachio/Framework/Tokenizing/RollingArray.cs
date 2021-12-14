using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Morestachio.Framework.Tokenizing;

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
		Buffer = new T[size];
		_writerIndex = -1;
	}

	private int _writerIndex;

	/// <summary>
	///		The internal used buffer
	/// </summary>
	protected readonly T[] Buffer;

	/// <summary>
	///		Gets the length of the <see cref="RollingArray{T}"/>
	/// </summary>
	public int Length
	{
		get
		{
			return Buffer.Length;
		}
	}

	/// <summary>
	///		Gets the current writer position within the bounds of <see cref="Buffer"/>
	/// </summary>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal int Pos()
	{
		return _writerIndex % Buffer.Length;
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
			return Get(index);
		}
	}

	/// <summary>
	///		Gets the char at the natural index
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public T Get(int index)
	{
		var inx = (_writerIndex % Buffer.Length + 1) + index;
		if (inx >= Buffer.Length)
		{
			//in this case we underflow the array length so set it back to the end of the array
			return Buffer[inx - Buffer.Length];
		}
		return Buffer[inx];
	}

	///// <summary>
	/////		Translates a natural index 0-Length to a real index depending on the current writer index
	///// </summary>
	///// <param name="index"></param>
	///// <returns></returns>
	//internal int Translate(int index)
	//{
			
	//}

	/// <summary>
	///		Returns the contents of the array
	/// </summary>
	/// <returns></returns>
	public T[] ToArray()
	{
		T[] arr;

		if (_writerIndex < Buffer.Length)
		{
			//if there are less written elements than the size of the buffer, omit those
			arr = new T[_writerIndex + 1];
			for (int i = 0; i < _writerIndex + 1; i++)
			{
				arr[i] = Buffer[i];
			}
		}
		else
		{
			arr = new T[Buffer.Length];
			for (int i = 0; i < Buffer.Length; i++)
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
		Buffer[_writerIndex % Buffer.Length] = c;
	}

	/// <summary>
	///		Checks if the elements are equal to the elements at the start of the array
	/// </summary>
	/// <param name="elements"></param>
	/// <returns></returns>
	public virtual bool StartsWith(T[] elements, IEqualityComparer<T> comparer = null)
	{
		if (elements.Length > Buffer.Length)
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
	public virtual bool EndsWith(T[] elements, IEqualityComparer<T> comparer = null)
	{
		if (elements.Length > Buffer.Length)
		{
			throw new IndexOutOfRangeException("The number of elements exceeds the size of the array");
		}
		for (int i = 0; i < elements.Length; i++)
		{
			var objA = elements[elements.Length - (i + 1)];
			var objB = this[Buffer.Length - (i + 1)];
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
			
		/// <inheritdoc />
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
			
		/// <inheritdoc />
		public void Reset()
		{
			_index = 0;
		}
			
		/// <inheritdoc />
		public T Current { get; private set; }
			
		/// <inheritdoc />
		object IEnumerator.Current
		{
			get { return Current; }
		}
			
		/// <inheritdoc />
		public void Dispose()
		{
		}
	}
		
	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator()
	{
		return new RollingArrayEnumerator(this);
	}
		
	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}