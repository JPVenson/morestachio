using Morestachio.Framework.IO.SubStream;

namespace Morestachio.Framework.IO.SingleStream;

/// <summary>
///		Defines a <see cref="IByteCounterStream"/> that does not target any output stream and will not store any data
/// </summary>
public class NullStream : IByteCounterStream
{
	/// <summary>
	///		Creates a new <see cref="NullStream"/>
	/// </summary>
	/// <param name="options"></param>
	public NullStream(ParserOptions options)
	{
		Options = options;
	}
	
	/// <summary>
	/// 
	/// </summary>
	protected ParserOptions Options { get; }

	/// <inheritdoc />
	public void Dispose()
	{
		
	}

	/// <inheritdoc />
	public long BytesWritten { get; }

	/// <inheritdoc />
	public bool ReachedLimit { get; }
#if Span
/// <inheritdoc />
	public void Write(ReadOnlyMemory<char> content)
	{
		
	}
#endif

	/// <inheritdoc />
	public void Write(in string content)
	{
		
	}

	/// <inheritdoc />
	public ISubByteCounterStream GetSubStream()
	{
		return new SubByteCounterStream(this, Options);
	}
}
