using System;

namespace Morestachio
{
	/// <summary>
	///		An Exception that occured on Runtime
	/// </summary>
	public class MorestachioRuntimeException : Exception
	{
		/// <inheritdoc />
		public MorestachioRuntimeException(string message) : base("Morestachio Runtime error:" + message)
		{
			
		}
	}

	/// <summary>
	///		An Exception that occured on Parsing
	/// </summary>
	public class MorestachioParserException : Exception
	{
		/// <inheritdoc />
		public MorestachioParserException(string message) : base("Morestachio Parser error:" + message)
		{
			
		}
	}
}
