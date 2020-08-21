namespace Morestachio.Document.TextOperations
{
	/// <summary>
	///		The list of Text operation types
	/// </summary>
	public enum TextOperationTypes
	{
		/// <summary>
		///		Writes a LineBreak
		/// </summary>
		LineBreak,

		/// <summary>
		///		Trims a number(or all) LineBreaks that follows
		/// </summary>
		TrimLineBreaks,

		/// <summary>
		///		Trims all Whitespace characters at the start of a line as long as its disabled
		/// </summary>
		ContinuesTrimming,
	}
}