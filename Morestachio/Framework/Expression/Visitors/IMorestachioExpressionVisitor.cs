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

		/// <summary>
		///		Visits an Operator
		/// </summary>
		void Visit(MorestachioLambdaExpression expression);
	}

	/// <summary>
	///		Base class for visiting expressions
	/// </summary>
	public class MorestachioExpressionVisitorBase : IMorestachioExpressionVisitor
	{
		/// <inheritdoc />
		public virtual void Visit(MorestachioExpression expression)
		{
			foreach (var expressionArgument in expression.Formats)
			{
				this.Visit(expressionArgument);
			}
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioArgumentExpressionList expression)
		{
			foreach (var expressionArgument in expression.Expressions)
			{
				this.Visit(expressionArgument);
			}
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioBracketExpression expression)
		{
			foreach (var expressionArgument in expression.Expressions)
			{
				this.Visit(expressionArgument);
			}
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioMultiPartExpressionList expression)
		{
			foreach (var expressionArgument in expression.Expressions)
			{
				this.Visit(expressionArgument);
			}
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioExpressionString expression)
		{
		}
		
		/// <inheritdoc />
		public virtual void Visit(ExpressionArgument expression)
		{
			this.Visit(expression.MorestachioExpression);
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioExpressionNumber expression)
		{
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioOperatorExpression expression)
		{
			this.Visit(expression.LeftExpression);
			if (expression.RightExpression != null)
			{
				this.Visit(expression.RightExpression);
			}
		}
		
		/// <inheritdoc />
		public virtual void Visit(MorestachioLambdaExpression expression)
		{
			this.Visit(expression.Expression);
			this.Visit(expression.Parameters);
		}
	}
}