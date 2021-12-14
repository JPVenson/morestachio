namespace Morestachio.Framework.Context.Options;

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