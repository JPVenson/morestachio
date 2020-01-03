using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace Morestachio.Framework
{
	/// <summary>
	///		Internal class to ensure that the given limit of bytes to write is never extended to ensure template quotas
	/// </summary>
	/// <seealso cref="System.IDisposable" />
	internal class ByteCounterStream : IByteCounterStream
	{
		private readonly ParserOptions _options;

		public ByteCounterStream([NotNull] Stream stream, 
			[NotNull] Encoding encoding, 
			int bufferSize, 
			bool leaveOpen,
			ParserOptions options)
		{
			_options = options;
			BaseWriter = new StreamWriter(stream, encoding, bufferSize, leaveOpen);
		}

		public StreamWriter BaseWriter { get; set; }

		public long BytesWritten { get; private set; }
		public bool ReachedLimit { get; private set; }

		public void Write(string content)
		{
			content = content ?? _options.Null?.ToString();

			var sourceCount = BytesWritten;

			if (_options.MaxSize == 0)
			{
				BaseWriter.Write(content);
				return;
			}

			if (sourceCount >= _options.MaxSize - 1)
			{
				ReachedLimit = true;
				return;
			}
			
			//TODO this is a performance critical operation. As we might deal with variable-length encodings this cannot be compute initial
			var cl = _options.Encoding.GetByteCount(content);

			var overflow = sourceCount + cl - _options.MaxSize;
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

		public void Dispose()
		{
			BaseWriter.Flush();
			BaseWriter.Dispose();
		}
	}
}