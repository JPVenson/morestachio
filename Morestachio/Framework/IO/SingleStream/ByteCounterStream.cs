using System;
using System.IO;
using Morestachio.Framework.IO.SubStream;

namespace Morestachio.Framework.IO.SingleStream
{
	/// <summary>
	///		Internal class to ensure that the given limit of bytes to write is never extended to ensure template quotas
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	public class ByteCounterStream : IByteCounterStream
	{
		/// <summary>
		///		The target stream
		/// </summary>
		public Stream Stream { get; }

		/// <summary>
		/// 
		/// </summary>
		protected ParserOptions Options { get; }

		/// <summary>
		/// 
		/// </summary>
		public ByteCounterStream(Stream stream,
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

#if Span
		/// <inheritdoc />
		public void Write(ReadOnlyMemory<char> content)
		{
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
			var cl = Options.Encoding.GetByteCount(content.Span);

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
				//BaseWriter.Write(content.ToCharArray(0, (int)(cl - overflow)));
				BaseWriter.Write(content[0..((int)(cl - overflow))]);
			}
			else
			{
				BytesWritten += cl;
				BaseWriter.Write(content);
			}
		}
#endif
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
}