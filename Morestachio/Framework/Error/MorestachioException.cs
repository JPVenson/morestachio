using System;

namespace Morestachio.Framework.Error;

/// <summary>
///     The General Exception type for Framework Exceptions
/// </summary>
public class MorestachioException : Exception
{
	/// <summary>
	///     Ctor
	/// </summary>
	/// <param name="message"></param>
	public MorestachioException(string message) 
		: base(message)
	{
	}
}