using System;
using System.IO;
using Morestachio.Document;

namespace Morestachio.Framework.IO.MultiPart;

/// <summary>
///		Defines a seperate part of the template that can be processed individually
/// </summary>
public interface IByteCounterStreamPart : IDisposable
{
	/// <summary>
	///		The shared info with all other parts of this template
	/// </summary>
	ByteCounterInfo Info { get; }

	/// <summary>
	///		The state this counter is in
	/// </summary>
	ByteCounterStreamPartType State { get; }

	///  <summary>
	/// 		Writes the Content into the underlying Stream when the limit is not exceeded
	///  </summary>
#if Span
	void Write(ReadOnlySpan<char> content);
#else
	void Write(string content);
#endif

	/// <summary>
	///		Direct access to the underlying stream
	/// </summary>
	/// <returns></returns>
	Stream BaseStream();
}