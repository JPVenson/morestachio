using System;
using System.IO;
using System.Threading;
using Morestachio.Document.Contracts;

namespace Morestachio.Framework
{
	public interface IByteCounterStreamPart : IDisposable
	{
		ByteCounterInfo Info { get; set; }

		ByteCounterStreamPartType State { get; }

		/// <summary>
		///		Writes the Content into the underlying Stream when the limit is not exceeded
		/// </summary>
		/// <param name="content"></param>
		void Write(string content);
		Stream BaseStream();
	}

	public enum ByteCounterStreamPartType
	{
		Open,
		Writing,
		Closed
	}

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

	/// <summary>
	///		Defines the output that can count on written bytes into a stream
	/// </summary>
	public interface IByteCounterStream : IDisposable
	{
		/// <summary>
		/// Gets or sets the bytes written.
		/// </summary>
		/// <value>
		/// The bytes written.
		/// </value>
		long BytesWritten { get; }

		/// <summary>
		/// Gets or sets a value indicating whether [reached limit].
		/// </summary>
		/// <value>
		///   <c>true</c> if [reached limit]; otherwise, <c>false</c>.
		/// </value>
		bool ReachedLimit { get; }


		/// <summary>
		///		Writes the Content into the underlying Stream when the limit is not exceeded
		/// </summary>
		/// <param name="content"></param>
		void Write(string content);

		/// <summary>
		///		Gets an <see cref="IByteCounterStream"/> that keeps tracking of the bytes written and will stop buffering bytes if the <see cref="ParserOptions.MaxSize"/> is reached but will not write into the parent stream
		/// </summary>
		/// <returns></returns>
		ISubByteCounterStream GetSubStream();

		Stream Stream { get; }
	}
	

	/// <summary>
	///		This kind of stream can be used to buffer the contents of one or more <see cref="IDocumentItem.Render"/> calls without writing into the main stream but with taking Size into account
	/// </summary>
	public interface ISubByteCounterStream : IByteCounterStream
	{
		/// <summary>
		///		Reads the contents of the underlying stream
		/// </summary>
		/// <returns></returns>
		string Read();
	}
}