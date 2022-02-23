namespace Morestachio.Framework.Error;

/// <summary>
///		The Infinite Partials Exception type
/// </summary>
public class MorestachioStackOverflowException : MorestachioException
{
	/// <summary>
	/// 
	/// </summary>
	public MorestachioStackOverflowException(string message) : base(message)
	{
	}
}