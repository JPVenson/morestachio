namespace Morestachio.Framework.IO.MultiPart;

/// <summary>
///		Defines the state of the <see cref="IByteCounterStreamPart"/>
/// </summary>
public enum ByteCounterStreamPartType
{
	/// <summary>
	///		The <see cref="IByteCounterStreamPart"/> is open and can be written to
	/// </summary>
	Open,

	/// <summary>
	///		the <see cref="IByteCounterStreamPart"/> is currently written to
	/// </summary>
	Writing,

	/// <summary>
	///		the <see cref="IByteCounterStreamPart"/> cannot be written to anymore
	/// </summary>
	Closed
}