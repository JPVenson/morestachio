namespace Morestachio.Framework.Context.Options
{
	/// <summary>
	///		Declares the destination of an embedded statement like the whitespace control
	/// </summary>
	public enum EmbeddedInstructionOrigin
	{
		/// <summary>
		///		Indicates that the embedded statement is standing on its own without attachment to any other statement
		/// </summary>
		Self,

		/// <summary>
		///		Indicates that the embedded statement was created by the previous token
		/// </summary>
		Previous,

		/// <summary>
		///		Indicates that the embedded statement was created by the next token
		/// </summary>
		Next
	}
}