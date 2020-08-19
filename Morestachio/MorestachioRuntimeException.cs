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
}
