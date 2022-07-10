using System.Collections.Generic;
using Morestachio.Document;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Util;

namespace Morestachio.Framework.Context;

/// <summary>
///     The context object that will be used for the root of
/// </summary>
public class PartialContextObject : ContextObject
{
	/// <inheritdoc />
	public PartialContextObject(string key, ContextObject parent, object value) : base(key, parent, value)
	{
	}

	/// <inheritdoc />
	public override ContextObject HandlePathContext(ref KeyValuePair<string, PathType> currentElement,
													IMorestachioExpression morestachioExpression,
													ScopeData scopeData)
	{
		if (currentElement.Value != PathType.DataPath || !currentElement.Key.StartsWith('$'))
		{
			return null;
		}

		object value = null;

		if (currentElement.Key.Equals("$recursion"))
		{
			value = scopeData.PartialDepth.Count;
		}

		return value == null ? null : scopeData.ParserOptions.CreateContextObject(currentElement.Key, value, this);
	}
}