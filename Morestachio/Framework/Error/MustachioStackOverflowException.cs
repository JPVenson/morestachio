namespace Morestachio.Framework.Error
{
	/// <summary>
	///		The Infinite Partials Exception type
	/// </summary>
	public class MustachioStackOverflowException : MustachioException
	{
		/// <summary>
		/// 
		/// </summary>
		public MustachioStackOverflowException(string message) : base(message)
		{
		}
	}
}