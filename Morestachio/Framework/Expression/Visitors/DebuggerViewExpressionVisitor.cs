using System;
using System.Linq;
using Morestachio.Framework.Expression.Framework;

namespace Morestachio.Framework.Expression.Visitors
{
	/// <summary>
	///		Builds expressions for the debugger viewer
	/// </summary>
	public class DebuggerViewExpressionVisitor : IMorestachioExpressionVisitor
	{
		/// <summary>
		/// 
		/// </summary>
		public DebuggerViewExpressionVisitor()
		{
			StringBuilder = new StringBuilderInterlaced<NoColor>();
		}

		/// <summary>
		/// 
		/// </summary>
		public StringBuilderInterlaced<NoColor> StringBuilder { get; set; }

		/// <inheritdoc />
		public void Visit(MorestachioExpression expression)
		{
			StringBuilder.Append("Exp({");

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
				StringBuilder.Append("} => ");
				if (expression.FormatterName == string.Empty)
				{
					StringBuilder.Append("[None]");
				}
				else
				{
					StringBuilder.Append(expression.FormatterName);	
				}
				StringBuilder.Append("(");
				

				if (expression.Formats.Any())
				{
					StringBuilder.Up().AppendLine().AppendInterlaced();
					for (var index = 0; index < expression.Formats.Count; index++)
					{
						//StringBuilder.Up();
						var expressionArgument = expression.Formats[index];
						Visit(expressionArgument);
						if (index != expression.Formats.Count - 1)
						{
							StringBuilder.Append(", ")
								.AppendLine()
								.AppendInterlaced();
						}
					}

					StringBuilder
						.Down()
						.Append(")")
						.AppendLine()
						.AppendInterlaced();
				}
				else
				{
					StringBuilder
						.Append(")");
				}
			}
			else
			{
				StringBuilder.Append("}");
			}
			StringBuilder.Append(")");
		}

		/// <summary>
		///		Gets the string representation of all visited expressions
		/// </summary>
		public string Text
		{
			get
			{
				return StringBuilder.ToString();
			}
		}

		/// <inheritdoc />
		public void Visit(MorestachioExpressionList expression)
		{
			StringBuilder
				.AppendLine("List[")
				.Up()
				.AppendInterlaced();;
			for (var index = 0; index < expression.Expressions.Count; index++)
			{
				var expressionExpression = expression.Expressions[index];
				this.Visit(expressionExpression);
				if (index != expression.Expressions.Count - 1)
				{
					StringBuilder.AppendLine(",").AppendInterlaced();
				}
			}

			StringBuilder.Down()
				.AppendLine()
				.AppendInterlaced("]");
		}

		/// <inheritdoc />
		public void Visit(MorestachioExpressionString expression)
		{
			StringBuilder.Append("String(");
			StringBuilder.Append(expression.Delimiter.ToString());
			foreach (var expressionStringConstPart in expression.StringParts)
			{
				StringBuilder.Append("const(");
				StringBuilder.Append(expressionStringConstPart.PartText);
				StringBuilder.Append(")");
			}
			StringBuilder.Append(expression.Delimiter.ToString());
			StringBuilder.Append(")");
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
			StringBuilder.Append("Number(");
			StringBuilder.Append("(" + expression.Number.Value.GetTypeCode() + ")");
			StringBuilder.Append(expression.Number.Value.ToString());
			StringBuilder.Append(")");
		}

		/// <inheritdoc />
		public void Visit(MorestachioOperatorExpression expression)
		{
			StringBuilder
				.Append("op(" + expression.Operator.OperatorText + ")") 
				.AppendLine("{")
				.Up()
				.AppendInterlacedLine("Left")
				.AppendInterlacedLine("{")
				.Up()
				.AppendInterlaced();

			this.Visit(expression.LeftExpression);
			StringBuilder
				.Down()
				.AppendLine()
				.AppendInterlaced("}");

			if (expression.RightExpression != null)
			{
				StringBuilder.AppendLine(",")
					.AppendInterlacedLine("Right")
					.AppendInterlacedLine("{")
					.Up()
					.AppendInterlaced();
				this.Visit(expression.RightExpression);
				StringBuilder.Down().AppendLine().AppendInterlaced("}");
			}
			StringBuilder.Down().AppendLine().AppendInterlaced("}");
		}
	}
}