namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Defines how to handle exceptions that are thrown by formatters
	/// </summary>
	public enum FormatterServiceExceptionHandling
	{
		/// <summary>
		///		Rethrows exceptions and stop execution
		/// </summary>
		ThrowExceptions,

		/// <summary>
		///		Handles exceptions and dismisses them
		/// </summary>
		IgnoreSilently,

		/// <summary>
		///		Returns exceptions as part of the result
		/// </summary>
		PrintExceptions
	}
}