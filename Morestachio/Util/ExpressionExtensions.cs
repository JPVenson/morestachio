using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Util;

/// <summary>
///		Contains methods for rendering an Expression as a string using various Expression visitors
/// </summary>
public static class ExpressionExtensions
{
	/// <summary>
	///		Uses the <see cref="ToParsableStringExpressionVisitor"/> to render the expression
	/// </summary>
	/// <param name="expression"></param>
	/// <returns></returns>
	public static string AsStringExpression(this IMorestachioExpression expression)
	{
		var sb = StringBuilderCache.Acquire(90);
		var toParsableStringExpressionVisitor = new ToParsableStringExpressionVisitor(sb);
		toParsableStringExpressionVisitor.Visit(expression);

		return StringBuilderCache.GetStringAndRelease(sb);
	}

	/// <summary>
	///		Uses the <see cref="ToParsableStringExpressionVisitor"/> to render the expression
	/// </summary>
	/// <param name="expression"></param>
	/// <returns></returns>
	public static string AsDebugExpression(this IMorestachioExpression expression)
	{
		var toParsableStringExpressionVisitor = new DebuggerViewExpressionVisitor();
		toParsableStringExpressionVisitor.Visit(expression);
		return toParsableStringExpressionVisitor.Text;
	}
}