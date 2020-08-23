using Morestachio.Framework.Error;

namespace Morestachio.Framework.Context.Options
{
	/// <summary>
	///		Defines how Morestachio Block should handle scoping in the presents of an alias
	/// </summary>
	public enum ScopingBehavior
	{
		/// <summary>
		///		Even if an Alias is present set the scope of the current block to its expression
		/// </summary>
		ScopeAnyway,

		/// <summary>
		///		If an Alias is present do nothing
		/// </summary>
		DoNotScope
	}

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