using System.Collections.Generic;
using Morestachio.Framework.Expression.Framework;

namespace Morestachio.TemplateContainers;

/// <summary>
///		Defines a storage used to obtain token matches from a string or other source. Use the <see cref="TemplateContainerBase"/> or <see cref="LazyTemplateContainerBase"/> as base class for your own implementation
/// </summary>
public interface ITemplateContainer
{
	/// <summary>
	///		Gets Tokens from the store
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	IEnumerable<TokenMatch> Matches(TokenzierContext context);
}