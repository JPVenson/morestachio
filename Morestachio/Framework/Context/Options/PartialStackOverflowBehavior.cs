using Morestachio.Framework;

namespace Morestachio
{
	/// <summary>
	///		Defines how the Parser should behave when encountering a the PartialStackSize to be exceeded
	/// </summary>
	public enum PartialStackOverflowBehavior
	{
		/// <summary>
		///		Throw a <see cref="MustachioStackOverflowException"/>
		/// </summary>
		FailWithException,
		/// <summary>
		///		Do nothing and skip further calls
		/// </summary>
		FailSilent
	}
}