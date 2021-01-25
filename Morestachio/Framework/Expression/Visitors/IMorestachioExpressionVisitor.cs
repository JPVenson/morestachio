namespace Morestachio.Framework.Expression.Visitors
{
	/// <summary>
	///		The Visitor interface for Expressions
	/// </summary>
	public interface IMorestachioExpressionVisitor
	{
		/// <summary>
		///		Visits an Expression
		/// </summary>
		void Visit(MorestachioExpression expression);
		
		/// <summary>
		///		Visits an Expression List
		/// </summary>
		void Visit(MorestachioArgumentExpressionList expression);
		/// <summary>
		///		Visits an Bracket enclosing one expression
		/// </summary>
		void Visit(MorestachioBracketExpression expression);
		
		/// <summary>
		///		Visits an Expression List
		/// </summary>
		void Visit(MorestachioMultiPartExpressionList expression);
		
		/// <summary>
		///		Visits an Expression String
		/// </summary>
		void Visit(MorestachioExpressionString expression);
		
		/// <summary>
		///		Visits an Expression Argument
		/// </summary>
		void Visit(ExpressionArgument expression);
		
		/// <summary>
		///		Visits an Expression Number
		/// </summary>
		void Visit(MorestachioExpressionNumber expression);

		/// <summary>
		///		Visits an Operator
		/// </summary>
		void Visit(MorestachioOperatorExpression expression);
	}
}