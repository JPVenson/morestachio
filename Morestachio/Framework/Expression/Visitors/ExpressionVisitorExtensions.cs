using System;

namespace Morestachio.Framework.Expression.Visitors
{
	/// <summary>
	/// 
	/// </summary>
	public static class ExpressionVisitorExtensions
	{
		/// <summary>
		///		Renders a Expression into the visitor
		/// </summary>
		public static void Visit(this IMorestachioExpressionVisitor visitor, IMorestachioExpression morestachioExpression)
		{
			switch (morestachioExpression)
			{
				case MorestachioExpression expression1:
					visitor.Visit(expression1);
					break;
				case ExpressionArgument expressionArgument:
					visitor.Visit(expressionArgument);
					break;
				case MorestachioArgumentExpressionList expressionList:
					visitor.Visit(expressionList);
					break;
				case MorestachioMultiPartExpressionList expressionList:
					visitor.Visit(expressionList);
					break;
				case MorestachioExpressionString expressionString:
					visitor.Visit(expressionString);
					break;
				case MorestachioExpressionNumber expressionNumber:
					visitor.Visit(expressionNumber);
					break;
				case MorestachioOperatorExpression operatorExpression:
					visitor.Visit(operatorExpression);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(morestachioExpression));
			}
		}
	}
}