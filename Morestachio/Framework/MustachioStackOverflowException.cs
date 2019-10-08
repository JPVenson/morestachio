namespace Morestachio.Framework
{
	/// <summary>
	///		The Infinite Partials Exception type
	/// </summary>
	public class MustachioStackOverflowException : MustachioException
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="message"></param>
		/// <param name="replacements"></param>
		public MustachioStackOverflowException(string message) : base(message)
		{
		}
	}
}