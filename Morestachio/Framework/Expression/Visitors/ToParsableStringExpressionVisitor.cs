using System;
using System.Collections.Generic;
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
					case PathType.ObjectSelector:
						StringBuilder.Append("?");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			if (expression.FormatterName != null)
			{
				if (!isSelfAssignment)
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
		public void Visit(MorestachioMultiPartExpressionList expression)
		{
			for (var index = 0; index < expression.Expressions.Count; index++)
			{
				var expressionExpression = expression.Expressions[index];

				if (index != 0
					&& !(expressionExpression is MorestachioExpression exp
						 && (exp.PathParts.Current.Value == PathType.SelfAssignment 
						     || 
						     (exp.PathParts.Current.Value == PathType.DataPath && exp.PathParts.Current.Key is null))))
				{
					StringBuilder.Append(".");
				}
				this.Visit(expressionExpression);
			}
		}

		/// <inheritdoc />
		public void Visit(MorestachioExpressionString expression)
		{
			StringBuilder.Append(expression.Delimiter +
								 string.Join("", expression.StringParts.Select(f => f.PartText))
								 + expression.Delimiter);
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
			//if (!(expression.LeftExpression is MorestachioOperatorExpression)
			//&& !(expression.LeftExpression is MorestachioExpressionListBase expList 
			//    && expList.Expressions.Count == 1 
			//    && (expList.Expressions.FirstOrDefault() is MorestachioOperatorExpression)))
			//{
			//	this.Visit(expression.LeftExpression);
			//}

			this.Visit(expression.LeftExpression);
			StringBuilder.Append(" ");
			StringBuilder.Append(expression.Operator.OperatorText);
			if (expression.RightExpression != null)
			{
				StringBuilder.Append(" ");
				this.Visit(expression.RightExpression);
			}
		}
	}
}
