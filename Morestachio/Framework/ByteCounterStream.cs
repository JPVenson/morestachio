using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;

namespace Morestachio.Framework
{
	public class PartialByteCounterStream : IByteCounterStream
	{
		public PartialByteCounterStream(ParserOptions options)
		{
			Options = options;
			Partials = new List<IByteCounterStreamPart>();
			Info = new ByteCounterInfo(options.MaxSize);
			RootPart = GetNewPart(options.StreamFactory.Output());
		}
		
		public ParserOptions Options { get; }
		public IList<IByteCounterStreamPart> Partials { get; private set; }

		public IByteCounterStreamPart RootPart { get; set; }
		private ByteCounterInfo Info { get; }
		
		public IByteCounterStreamPart GetNewPart(Stream withStream)
		{
			var partialByteCounterStreamPart = new PartialByteCounterStreamPart(withStream, 2024, Options, this);
			Partials.Add(partialByteCounterStreamPart);
			return partialByteCounterStreamPart;
		}

		public IByteCounterStreamPart GetNewPart()
		{
			return GetNewPart(Options.StreamFactory.TempStream());
		}

		public void Dispose()
		{
			foreach (var byteCounterStream in Partials)
			{
				byteCounterStream.Dispose();
			}
		}

		public void Flush()
		{
			try
			{
				if (!Monitor.TryEnter(this))
				{
					return;
				}

				foreach (var byteCounterStreamPart in Partials.ToArray())
				{
					if (byteCounterStreamPart.State == ByteCounterStreamPartType.Closed)
					{
						using (var baseWriterBaseStream = byteCounterStreamPart.BaseStream())
						{
							baseWriterBaseStream.CopyTo(RootPart.BaseStream());
						}
					}
					Partials.RemoveAt(0);
				}
			}
			finally
			{
				if (Monitor.IsEntered(this))
				{
					Monitor.Exit(this);
				}
			}
		}

		public long BytesWritten
		{
			get
			{
				return Info.BytesWritten;
			}
		}

		public bool ReachedLimit
		{
			get
			{
				return Info.ReachedLimit;
			}
		}

		public void Write(string content)
		{
			RootPart.Write(content);
		}

		public ISubByteCounterStream GetSubStream()
		{
			return new SubByteCounterStream(this, Options);
		}

		public Stream Stream
		{
			get
			{
				return RootPart.BaseStream();
			}
		}
	}

	public class PartialByteCounterStreamPart : IByteCounterStreamPart
	{
		private readonly PartialByteCounterStream _partialByteCounterStream;
		private ByteCounterStreamPartType _state;
		public ParserOptions Options { get; }

		/// <inheritdoc />
		public ByteCounterStreamPartType State
		{
			get { return _state; }
			private set { _state = value; }
		}

		public PartialByteCounterStreamPart([NotNull] Stream stream,
			int bufferSize,
			ParserOptions options,
			PartialByteCounterStream partialByteCounterStream)
		{
			if (!stream.CanWrite)
			{
				throw new InvalidOperationException($"The stream '{stream.GetType()}' is ReadOnly.");
			}
			_partialByteCounterStream = partialByteCounterStream;
			Options = options;
			_state = ByteCounterStreamPartType.Open;
			BaseWriter = new StreamWriter(stream, options.Encoding, bufferSize, false);
		}

		public StreamWriter BaseWriter { get; set; }

		public void Dispose()
		{
			_state = ByteCounterStreamPartType.Closed;
			BaseWriter.Flush();
			_partialByteCounterStream.Flush();
		}
		
		public Stream BaseStream()
		{
			return BaseWriter.BaseStream;
		}

		public ByteCounterInfo Info { get; set; }
		public void Write(string content)
		{
			if (_state == ByteCounterStreamPartType.Closed)
			{
				throw new InvalidOperationException("The current state of this partial is closed and cannot be written to anymore");
			}

			content = content ?? Options.Null?.ToString();
			_state = ByteCounterStreamPartType.Writing;
			
			if (Options.MaxSize == 0)
			{
				BaseWriter.Write(content);
				return;
			}

			if (Info.ReachedLimit)
			{
				return;
			}

			//TODO this is a performance critical operation. As we might deal with variable-length encodings this cannot be compute initial
			var cl = Options.Encoding.GetByteCount(content);
			
			var overflow = Info.Increment(cl);
			if (overflow == cl)
			{
				BaseWriter.Write(content);
				return;
			}

			if (overflow == 0)
			{
				return;
			}

			BaseWriter.Write(content.ToCharArray(0, overflow));
		}
	}

	/// <summary>
	///		Internal class to ensure that the given limit of bytes to write is never extended to ensure template quotas
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class ByteCounterStream : IByteCounterStream
	{
		[NotNull] public Stream Stream { get; }

		/// <summary>
		/// 
		/// </summary>
		protected ParserOptions Options { get; }

		/// <summary>
		/// 
		/// </summary>
		public ByteCounterStream([NotNull] Stream stream,
			int bufferSize,
			bool leaveOpen,
			ParserOptions options)
		{
			if (!stream.CanWrite)
			{
				throw new InvalidOperationException($"The stream '{stream.GetType()}' is ReadOnly.");
			}

			Stream = stream;
			Options = options;
			BaseWriter = new StreamWriter(stream, options.Encoding, bufferSize, leaveOpen);
		}

		/// <summary>
		///		The target writer
		/// </summary>
		protected StreamWriter BaseWriter { get; set; }

		/// <inheritdoc />
		public long BytesWritten { get; protected set; }
		/// <inheritdoc />
		public bool ReachedLimit { get; protected set; }

		/// <inheritdoc />
		public void Write(string content)
		{
			content = content ?? Options.Null?.ToString();

			var sourceCount = BytesWritten;

			if (Options.MaxSize == 0)
			{
				BaseWriter.Write(content);
				return;
			}

			if (sourceCount >= Options.MaxSize - 1)
			{
				ReachedLimit = true;
				return;
			}

			//TODO this is a performance critical operation. As we might deal with variable-length encodings this cannot be compute initial
			var cl = Options.Encoding.GetByteCount(content);

			var overflow = sourceCount + cl - Options.MaxSize;
			if (overflow <= 0)
			{
				BytesWritten += cl;
				BaseWriter.Write(content);
				return;
			}

			if (overflow < content.Length)
			{
				BytesWritten += cl - overflow;
				BaseWriter.Write(content.ToCharArray(0, (int)(cl - overflow)));
			}
			else
			{
				BytesWritten += cl;
				BaseWriter.Write(content);
			}
		}

		/// <inheritdoc />
		public ISubByteCounterStream GetSubStream()
		{
			return new SubByteCounterStream(this, Options);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			BaseWriter?.Flush();
			BaseWriter?.Dispose();
		}
	}

	/// <summary>
	///		This stream can be used to buffer the contents of one or more <see cref="IDocumentItem.Render"/> calls without writing into the main stream but with taking Size into account
	/// </summary>
	public class SubByteCounterStream : ByteCounterStream, ISubByteCounterStream
	{
		private readonly IByteCounterStream _source;

		/// <summary>
		///		Creates a new SubByteCounterStream
		/// </summary>
		/// <param name="source"></param>
		/// <param name="options"></param>
		public SubByteCounterStream(
			IByteCounterStream source,
			ParserOptions options)
			: base(new MemoryStream(), 2024, false, options)
		{
			_source = source;
			BytesWritten = _source.BytesWritten;
			ReachedLimit = _source.ReachedLimit;
		}

		public string Read()
		{
			BaseWriter.Flush();
			return Options.Encoding.GetString((BaseWriter.BaseStream as MemoryStream).ToArray());
		}
	}
}