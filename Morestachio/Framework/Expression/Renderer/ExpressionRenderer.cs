using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morestachio.Framework.Expression.Renderer
{
	public static class ExpressionRenderer
	{
		public static StringBuilder RenderExpression(IExpression expression)
		{
			switch (expression)
			{
				case Expression expression1:
					return RenderExpression(expression1);
				case ExpressionArgument expressionArgument:
					return RenderExpression(expressionArgument);
				case ExpressionList expressionList:
					return RenderExpression(expressionList);
				case ExpressionString expressionString:
					return RenderExpression(expressionString);
				default:
					throw new ArgumentOutOfRangeException(nameof(expression));
			}
		}

		public static StringBuilder RenderExpression(Expression expression)
		{
			var text = new StringBuilder();

			foreach (var expressionPathPart in expression.PathParts)
			{
				switch (expressionPathPart.Value)
				{
					case PathTokenizer.PathType.DataPath:
						if (text.Length != 0)
						{
							text.Append(".");
						}
						text.Append(expressionPathPart.Key);
						break;
					case PathTokenizer.PathType.RootSelector:
						text.Append("~");
						break;
					case PathTokenizer.PathType.ParentSelector:
						text.Append("../");
						break;
					case PathTokenizer.PathType.SelfAssignment:
						text.Append(".");
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (expression.Formats.Any())
			{
				text.Append("(");
				text.Append(expression.Formats.Select(f =>
				{
					var argName = "";
					if (f.Name != null)
					{
						argName += $"[{f.Name}] ";
					}

					return argName + RenderExpression(f.Expression);
				}).Aggregate((e, f) => e + ", " + f));
				text.Append(")");
			}

			return text;
		}

		public static StringBuilder RenderExpression(ExpressionList expression)
		{
			var sb = new StringBuilder();
			sb.Append(string.Join(".", expression.Expressions.Select(RenderExpression).Select(f => f.ToString())));
			return sb;
		}

		public static StringBuilder RenderExpression(ExpressionString expression)
		{
			var sb = new StringBuilder();
			sb.Append(expression.Delimiter + string.Join("", expression.StringParts.Select(f => f.PartText)) + expression.Delimiter);
			return sb;
		}
	}
}
