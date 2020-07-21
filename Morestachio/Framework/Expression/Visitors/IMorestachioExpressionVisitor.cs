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
		void Visit(MorestachioExpressionList expression);
		
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
		void Visit(ExpressionNumber expression);
	}
}