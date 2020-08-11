using System;
using System.IO;
using JetBrains.Annotations;

namespace Morestachio.Framework
{
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
}