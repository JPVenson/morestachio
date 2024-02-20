using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Fluent.Expression;

/// <summary>
///		Gets the methods available at the end of an property call
/// </summary>
public class PropertyMorestachioExpressionBuilder : MorestachioExpressionBuilder
{
	internal PropertyMorestachioExpressionBuilder(IList<IMorestachioExpression> parts, int column)
	{
		ExpressionParts = parts;
		Column = column;
	}

	/// <summary>
	///		Calls an formatter
	/// </summary>
	/// <param name="functionName"></param>
	/// <param name="arguments"></param>
	/// <returns></returns>
	public PropertyMorestachioExpressionBuilder Call(string functionName,
													Func<MorestachioArgumentExpressionBuilder,
														MorestachioArgumentExpressionBuilder> arguments = null)
	{
		MorestachioExpression targetExpression;

		if (ExpressionParts.LastOrDefault() is MorestachioExpression exp && exp.FormatterName == null)
		{
			exp.FormatterName = functionName;
			targetExpression = exp;
		}
		else
		{
			targetExpression = new MorestachioExpression(TextRange.Unknown);
			targetExpression.FormatterName = functionName;
			ExpressionParts.Add(targetExpression);
		}

		if (arguments != null)
		{
			foreach (var argument in arguments(new MorestachioArgumentExpressionBuilder()).Arguments)
			{
				targetExpression.Formats.Add(new ExpressionArgument(TextRange.Unknown, argument.Value, argument.Key));
			}
		}

		return this;
	}
}