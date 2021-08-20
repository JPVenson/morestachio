using System;
using System.Linq;
using System.Text;
using Morestachio.Framework.Expression.Framework;

namespace Morestachio.Framework.Expression.Visitors
{
	/// <summary>
	///		Visits all Expression in a chain and creates an string inside the StringBuilder property that looks like the original expressions text form
	/// </summary>
	public class ToParsableStringExpressionVisitor : IMorestachioExpressionVisitor
	{
		/// <summary>
		/// 
		/// </summary>
		public ToParsableStringExpressionVisitor()
		{
			StringBuilder = new StringBuilder();
		}

		/// <summary>
		///		If set to true any found End-Of-Expression delimiters will be truncated from the output
		/// </summary>
		public bool TrimEoexDelimiters { get; set; }

		/// <summary>
		///		The created string
		/// </summary>
		public StringBuilder StringBuilder { get; set; }

		/// <inheritdoc />
		public void Visit(MorestachioExpression expression)
		{
			var isSelfAssignment = false;
			var expressionPathParts = expression.PathParts.ToArray();
			for (var index = 0; index < expressionPathParts.Length; index++)
			{
				var expressionPathPart = expressionPathParts[index];
				switch (expressionPathPart.Value)
				{
					case PathType.DataPath:
					//case PathType.Number:
					case PathType.Boolean:
						StringBuilder.Append(expressionPathPart.Key);

						if (index != expressionPathParts.Length - 1)
						{
							StringBuilder.Append(".");
						}
						break;
					case PathType.Null:
						StringBuilder.Append("null");
						break;
					case PathType.RootSelector:
						StringBuilder.Append("~");
						break;
					case PathType.ParentSelector:
						StringBuilder.Append("../");
						break;
					case PathType.SelfAssignment:
						StringBuilder.Append(".");
						isSelfAssignment = true;
						break;
					case PathType.ThisPath:
						StringBuilder.Append("this");
						if (index != expressionPathParts.Length - 1)
						{
							StringBuilder.Append(".");
						}
						break;
					case PathType.ObjectSelector:
						StringBuilder.Append("?");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			if (expression.FormatterName != null)
			{
				if (!isSelfAssignment && expressionPathParts.Length != 0)
				{
					StringBuilder.Append(".");
				}
				StringBuilder.Append(expression.FormatterName);
				StringBuilder.Append("(");

				if (expression.Formats.Any())
				{
					for (var index = 0; index < expression.Formats.Count; index++)
					{
						var expressionArgument = expression.Formats[index];
						Visit(expressionArgument);
						if (index != expression.Formats.Count - 1)
						{
							StringBuilder.Append(", ");
						}
					}
				}
				StringBuilder.Append(")");
			}

			if (expression.EndsWithDelimiter && !TrimEoexDelimiters)
			{
				StringBuilder.Append(";");
			}
		}

		/// <inheritdoc />
		public void Visit(MorestachioArgumentExpressionList expression)
		{
			for (var index = 0; index < expression.Expressions.Count; index++)
			{
				var expressionExpression = expression.Expressions[index];
				this.Visit(expressionExpression);
			}
		}
		
		/// <inheritdoc />
		public void Visit(MorestachioBracketExpression expression)
		{
			StringBuilder.Append("(");
			Visit(expression as MorestachioMultiPartExpressionList);
			StringBuilder.Append(")");
		}

		/// <inheritdoc />
		public void Visit(MorestachioMultiPartExpressionList expression)
		{
			for (var index = 0; index < expression.Expressions.Count; index++)
			{
				var expressionExpression = expression.Expressions[index];
				if (index > 0)
				{
					if (expressionExpression is MorestachioExpression exp)
					{
						var c = exp.PathParts.Current.Value;
						if (c != PathType.SelfAssignment)
						{
							StringBuilder.Append(".");
						}
					}
					else
					{
						StringBuilder.Append(".");
					}
				}

				//if (index != 0
				//	&& (expressionExpression is MorestachioExpression exp
				//		 && (exp.PathParts.Current.Value != PathType.SelfAssignment 
				//		     || 
				//		     (exp.PathParts.Current.Value == PathType.DataPath && exp.PathParts.Current.Key is null))))
				//{
				//	StringBuilder.Append(".");
				//}
				this.Visit(expressionExpression);
			}

			if (expression.EndsWithDelimiter && !TrimEoexDelimiters)
			{
				StringBuilder.Append(";");
			}
		}

		/// <inheritdoc />
		public void Visit(MorestachioExpressionString expression)
		{
			StringBuilder.Append(expression.Delimiter);
			foreach (var expressionStringConstPart in expression.StringParts)
			{
				var str = expressionStringConstPart.PartText.Replace(expression.Delimiter.ToString(), "\\" + expression.Delimiter);
				StringBuilder.Append(str);
			}
			StringBuilder.Append(expression.Delimiter);
		}

		/// <inheritdoc />
		public void Visit(ExpressionArgument expression)
		{
			if (!string.IsNullOrWhiteSpace(expression.Name))
			{
				StringBuilder.Append("[");
				StringBuilder.Append(expression.Name);
				StringBuilder.Append("] ");
			}

			this.Visit(expression.MorestachioExpression);
		}

		/// <inheritdoc />
		public void Visit(MorestachioExpressionNumber expression)
		{
			StringBuilder.Append(expression.Number.AsParsableString());
		}

		/// <inheritdoc />
		public void Visit(MorestachioOperatorExpression expression)
		{
			if (expression.Operator.IsBinaryOperator)
			{
				this.Visit(expression.LeftExpression);
				StringBuilder.Append(" ");
				StringBuilder.Append(expression.Operator.OperatorText);
				if (expression.RightExpression != null)
				{
					StringBuilder.Append(" ");
					this.Visit(expression.RightExpression);
				}
			}
			else
			{
				if (expression.Operator.Placement == OperatorPlacement.Left)
				{
					StringBuilder.Append(expression.Operator.OperatorText);
					this.Visit(expression.LeftExpression);
				}
				else
				{
					this.Visit(expression.LeftExpression);
					StringBuilder.Append(" ");
					StringBuilder.Append(expression.Operator.OperatorText);
				}
			}
		}

		/// <inheritdoc />
		public void Visit(MorestachioLambdaExpression expression)
		{
			StringBuilder.Append("(");
			if (expression.Parameters is MorestachioExpression exp)
			{
				this.Visit(exp);
			}
			else if (expression.Parameters is MorestachioBracketExpression argList)
			{
				for (var index = 0; index < argList.Expressions.Count; index++)
				{
					var arg = argList.Expressions[index];
					this.Visit(arg);
					if (index + 1 < argList.Expressions.Count)
					{
						StringBuilder.Append(", ");
					}
				}
			}
			StringBuilder.Append(") => ");
			this.Visit(expression.Expression);
		}
	}
}
