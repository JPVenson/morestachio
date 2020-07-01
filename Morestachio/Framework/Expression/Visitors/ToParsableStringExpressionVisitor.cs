using System;
using System.Linq;
using System.Text;

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
		
		/// <summary>
		///		Renders a Expression into the StringBuilder
		/// </summary>
		/// <param name="morestachioExpression"></param>
		public void Visit(IMorestachioExpression morestachioExpression)
		{
			switch (morestachioExpression)
			{
				case MorestachioExpression expression1:
					Visit(expression1);
					break;
				case ExpressionArgument expressionArgument:
					Visit(expressionArgument);
					break;
				case MorestachioExpressionList expressionList:
					Visit(expressionList);
					break;
				case MorestachioExpressionString expressionString:
					Visit(expressionString);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(morestachioExpression));
			}
		}
		
		/// <inheritdoc />
		public void Visit(MorestachioExpression expression)
		{
			var isSelfAssignment = false;
			for (var index = 0; index < expression.PathParts.Count; index++)
			{
				var expressionPathPart = expression.PathParts[index];
				switch (expressionPathPart.Value)
				{
					case PathType.DataPath:
					case PathType.Number:
					case PathType.Boolean:
						StringBuilder.Append(expressionPathPart.Key);

						if (index != expression.PathParts.Count - 1)
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
						isSelfAssignment = true;
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
		public void Visit(MorestachioExpressionList expression)
		{
			for (var index = 0; index < expression.Expressions.Count; index++)
			{
				var expressionExpression = expression.Expressions[index];
				if (index != 0 && 
				    (expressionExpression as MorestachioExpression)?.PathParts.All(f => f.Value == PathType.SelfAssignment) == false)
				{
					StringBuilder.Append(".");
				}
				Visit(expressionExpression);
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

			Visit(expression.MorestachioExpression);
		}
	}
}
