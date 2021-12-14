using System.Threading;

namespace Morestachio.Framework.IO.MultiPart;

/// <summary>
///		Defines the current state of the bytecounter
/// </summary>
public class ByteCounterInfo
{
	private readonly long _maxSize;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="maxSize"></param>
	public ByteCounterInfo(long maxSize)
	{
		_maxSize = maxSize;
	}

	private long _bytesWritten;
	private bool _reachedLimit;

	/// <summary>
	///		Gets the current number of bytes written
	/// </summary>
	public long BytesWritten
	{
		get { return _bytesWritten; }
	}

	/// <summary>
	///		
	/// </summary>
	public bool ReachedLimit
	{
		get { return _reachedLimit; }
	}

	/// <summary>
	///		Increments the internal counter before something was written. Returns the number of bytes you can write without reaching the limit
	/// </summary>
	/// <param name="by"></param>
	/// <returns></returns>
	public int Increment(int by)
	{
		if (_reachedLimit)
		{
			return 0;
		}

		var added = Interlocked.Add(ref _bytesWritten, @by);
		if (_maxSize != -1 && _maxSize < added)
		{
			_reachedLimit = true;
			return (int) (added - _maxSize);
		}

		return by;
	}
}