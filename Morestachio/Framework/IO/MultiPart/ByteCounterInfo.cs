using System.Threading;

namespace Morestachio.Framework.IO.MultiPart
{
	public class ByteCounterInfo
	{
		private readonly long _maxSize;

		public ByteCounterInfo(long maxSize)
		{
			_maxSize = maxSize;
		}

		private long _bytesWritten;
		private bool _reachedLimit;

		public long BytesWritten
		{
			get { return _bytesWritten; }
		}

		public bool ReachedLimit
		{
			get { return _reachedLimit; }
		}

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
}