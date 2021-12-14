using System;
using System.IO;
using Morestachio.Framework.IO.SubStream;

namespace Morestachio.Framework.IO.SingleStream;

/// <summary>
///		Uses a <see cref="TextWriter"/> to write the template
/// </summary>
public class ByteCounterTextWriter : IByteCounterStream
{
	/// <summary>
	///		The target string writer
	/// </summary>
	public TextWriter Writer { get; }

	/// <summary>
	/// 
	/// </summary>
	/// <param name="stringWriter"></param>
	/// <param name="options"></param>
	public ByteCounterTextWriter(TextWriter stringWriter, ParserOptions options)
	{
		Writer = stringWriter;
		Options = options;
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return Writer.ToString();
	}

	protected ParserOptions Options { get; }

	/// <inheritdoc />
	public void Dispose()
	{
		Writer.Dispose();
	}
		
	/// <inheritdoc />
	public long BytesWritten { get; private set; }
		
	/// <inheritdoc />
	public bool ReachedLimit { get; private set; }
		
#if Span
	/// <inheritdoc />
	public void Write(ReadOnlyMemory<char> content)
	{
		if (Options.MaxSize == 0)
		{
			Writer.Write(content);
			return;
		}
		var sourceCount = BytesWritten;

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
			Writer.Write(content);
			return;
		}

		if (overflow < content.Length)
		{
			BytesWritten += cl - overflow;
			//BaseWriter.Write(content.ToCharArray(0, (int)(cl - overflow)));
			Writer.Write(content[0..((int)(cl - overflow))]);
		}
		else
		{
			BytesWritten += cl;
			Writer.Write(content);
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
			Writer.Write(content);
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
			Writer.Write(content);
			return;
		}

		if (overflow < content.Length)
		{
			BytesWritten += cl - overflow;
			Writer.Write(content.ToCharArray(0, (int)(cl - overflow)));
		}
		else
		{
			BytesWritten += cl;
			Writer.Write(content);
		}
	}

	public ISubByteCounterStream GetSubStream()
	{
		return new SubByteCounterStream(this, Options);
	}
}