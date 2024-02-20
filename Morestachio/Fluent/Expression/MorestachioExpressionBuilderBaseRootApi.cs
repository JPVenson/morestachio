using System.Collections.Generic;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.StringParts;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Fluent.Expression;

/// <summary>
///		Builds the base elements of an Expression like a number or Null
/// </summary>
public class MorestachioExpressionBuilderBaseRootApi : MorestachioExpressionBuilder
{
	internal MorestachioExpressionBuilderBaseRootApi()
	{
	}

	/// <summary>
	///		Helper for parsing an fully qualified expression
	/// </summary>
	/// <param name="expression"></param>
	/// <returns></returns>
	public MorestachioExpressionBuilder Parse(string expression)
	{
		var expressionParserResult
			= ExpressionParser.ParseExpression(expression, TokenzierContext.FromText(expression));
		ExpressionParts.Add(expressionParserResult.Expression);
		Column += expressionParserResult.SourceBoundary.RangeEnd.Index;
		return this;
	}

	/// <summary>
	///		Adds a number at the start of an expression
	/// </summary>
	/// <param name="number"></param>
	/// <returns></returns>
	public MorestachioExpressionBuilder Number(Number number)
	{
		ExpressionParts.Add(new MorestachioExpressionNumber(number, TextRange.Unknown));
		Column += number.AsParsableString().Length;
		return this;
	}

	/// <summary>
	///		Adds a number at the start of an expression
	/// </summary>
	public MorestachioExpressionBuilder String(string text)
	{
		ExpressionParts.Add(new MorestachioExpressionString(TextRange.Unknown, '\"')
		{
			StringParts =
			{
				new ExpressionStringConstPart(text, TextRange.Unknown)
			}
		});
		Column += text.Length;
		return this;
	}

	/// <summary>
	///		Returns the null value
	/// </summary>
	/// <returns></returns>
	public MorestachioExpressionBuilder Null()
	{
		var morestachioExpression = new MorestachioExpression(TextRange.Unknown);
		morestachioExpression.PathParts = new Traversable(new[]
		{
			new KeyValuePair<string, PathType>(null, PathType.Null)
		});

		ExpressionParts.Add(morestachioExpression);
		return this;
	}

	/// <summary>
	///		Jump to the root object for the expression
	/// </summary>
	/// <returns></returns>
	public MorestachioExpressionBuilder GoToRoot()
	{
		var morestachioExpression = new MorestachioExpression(TextRange.Unknown);
		morestachioExpression.PathParts = new Traversable(new[]
		{
			new KeyValuePair<string, PathType>(null, PathType.RootSelector)
		});
		ExpressionParts.Add(morestachioExpression);
		return this;
	}

	/// <summary>
	///		Goes one level up
	/// </summary>
	/// <returns></returns>
	public MorestachioParentPathExpressionBuilder GoToParent()
	{
		var morestachioExpression = new MorestachioExpression(TextRange.Unknown);
		morestachioExpression.PathParts = new Traversable(new[]
		{
			new KeyValuePair<string, PathType>(null, PathType.ParentSelector)
		});
		ExpressionParts.Add(morestachioExpression);
		return new MorestachioParentPathExpressionBuilder(ExpressionParts, Column, morestachioExpression);
	}
}