using System.Collections.Generic;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;

namespace Morestachio.Fluent.Expression;

/// <summary>
///		Allows going to the parent of the current context
/// </summary>
public class MorestachioParentPathExpressionBuilder : MorestachioExpressionBuilder
{
	private readonly MorestachioExpression _morestachioExpression;

	internal MorestachioParentPathExpressionBuilder(IList<IMorestachioExpression> expressionParts,
													in int column,
													MorestachioExpression morestachioExpression)
	{
		_morestachioExpression = morestachioExpression;
		Column = column;
		ExpressionParts = expressionParts;
	}

	/// <summary>
	///		Adds the ~ part to the expression
	/// </summary>
	/// <returns></returns>
	public MorestachioParentPathExpressionBuilder GoToParent()
	{
		_morestachioExpression.PathParts = _morestachioExpression.PathParts.Expand(new[]
		{
			new KeyValuePair<string, PathType>(null, PathType.ParentSelector)
		});
		return this;
	}
}