using System;

namespace Morestachio.Document.TextOperations
{
	/// <summary>
	///		Indicates the position of where the linebreaks should be removed with the <see cref="TrimLineBreakTextOperation"/>
	/// </summary>
	[Flags]
	public enum LineBreakTrimDirection
	{
		/// <summary>
		///		Default none
		/// </summary>
		None = 0,
		/// <summary>
		///		Should trim all linebreaks at the start of the next content
		/// </summary>
		Begin = 1 << 0,

		/// <summary>
		///		Should tirm all linebreaks at the end of the next content
		/// </summary>
		End = 1 << 1
	}
}