using Morestachio.Document.Contracts;

namespace Morestachio.Framework.IO.SubStream;

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