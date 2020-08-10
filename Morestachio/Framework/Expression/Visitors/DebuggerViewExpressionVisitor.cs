using System;
using System.Linq;
using System.Text;
using Morestachio.Framework.Expression.Framework;

namespace Morestachio.Framework.Expression.Visitors
{
	public class DebuggerViewExpressionVisitor : IMorestachioExpressionVisitor
	{
		public DebuggerViewExpressionVisitor()
		{
			StringBuilder = new StringBuilderInterlaced<NoColor>();
		}

		public StringBuilderInterlaced<NoColor> StringBuilder { get; set; }
		public int ExpressionCounter { get; set; }

		public void Visit(MorestachioExpression expression)
		{
			var isSelfAssignment = false;
			var exp = ExpressionCounter++;
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

		public string Text
		{
			get
			{
				return StringBuilder.ToString();
			}
		}

		public void Visit(MorestachioExpressionList expression)
		{
			var exp = ExpressionCounter++;
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

		public void Visit(MorestachioExpressionString expression)
		{
			var exp = ExpressionCounter++;
			StringBuilder.Append("String(");
			StringBuilder.Append(expression.Delimiter.ToString());
			foreach (var expressionStringConstPart in expression.StringParts)
			{
				var expPart = ExpressionCounter++;
				StringBuilder.Append("const(");
				StringBuilder.Append(expressionStringConstPart.PartText);
				StringBuilder.Append(")");
			}
			StringBuilder.Append(expression.Delimiter.ToString());
			StringBuilder.Append(")");
		}

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

		public void Visit(MorestachioExpressionNumber expression)
		{
			StringBuilder.Append("Number(");
			StringBuilder.Append("(" + expression.Number.Value.GetTypeCode() + ")");
			StringBuilder.Append(expression.Number.Value.ToString());
			StringBuilder.Append(")");
		}

		public void Visit(MorestachioOperatorExpression expression)
		{
			var exp = ExpressionCounter++;
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