using Morestachio.Framework.Error;

namespace Morestachio.Framework.Context.Options;

/// <summary>
///		Defines how the Parser should behave when encountering a the PartialStackSize to be exceeded
/// </summary>
public enum PartialStackOverflowBehavior
{
	/// <summary>
	///		Throw a <see cref="MorestachioStackOverflowException"/>
	/// </summary>
	FailWithException,

	/// <summary>
	///		Do nothing and skip further calls
	/// </summary>
	FailSilent
}