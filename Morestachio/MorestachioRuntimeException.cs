using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

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
