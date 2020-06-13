using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morestachio.Framework.Expression.Renderer
{
	public static class ExpressionRenderer
	{
		/// <summary>
		///		Renders a Expression into the StringBuilder
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(IExpression expression, StringBuilder sb)
		{
			switch (expression)
			{
				case Expression expression1:
					RenderExpression(expression1, sb);
					break;
				case ExpressionArgument expressionArgument:
					RenderExpression(expressionArgument, sb);
					break;
				case ExpressionList expressionList:
					RenderExpression(expressionList, sb);
					break;
				case ExpressionString expressionString:
					RenderExpression(expressionString, sb);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(expression));
			}
		}
		
		/// <summary>
		///		Renders the Argument like this:
		///		IF HAS NAME	-> "[" + NAME + "]" -> END + RenderExpression(Expression)
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(ExpressionArgument expression, StringBuilder sb)
		{
			if (!string.IsNullOrWhiteSpace(expression.Name))
			{
				sb.Append("[");
				sb.Append(expression.Name);
				sb.Append("] ");
			}

			RenderExpression(expression.Expression, sb);
		}

		/// <summary>
		///		Renders the Expression like this:
		///		EACH PATHPART -> [Path + "."], "~", "../", "." END + IF HAS FORMATTER -> IF NOT SELFASSIGNMENT -> "." END -> FormatterName + "(" + [EACH ARG -> RenderExpression(ExpressionArgument ARG) + ", "] + ")" END
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(Expression expression, StringBuilder sb)
		{
			var isSelfAssignment = false;
			for (var index = 0; index < expression.PathParts.Count; index++)
			{
				var expressionPathPart = expression.PathParts[index];
				switch (expressionPathPart.Value)
				{
					case PathTokenizer.PathType.DataPath:
						sb.Append(expressionPathPart.Key);

						if (index != expression.PathParts.Count - 1)
						{
							sb.Append(".");
						}
						break;
					case PathTokenizer.PathType.RootSelector:
						sb.Append("~");
						break;
					case PathTokenizer.PathType.ParentSelector:
						sb.Append("../");
						break;
					case PathTokenizer.PathType.SelfAssignment:
						sb.Append(".");
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
					sb.Append(".");
				}

				sb.Append(expression.FormatterName);
				sb.Append("(");

				if (expression.Formats.Any())
				{
					for (var index = 0; index < expression.Formats.Count; index++)
					{
						var expressionArgument = expression.Formats[index];
						RenderExpression(expressionArgument, sb);
						if (index != expression.Formats.Count - 1)
						{
							sb.Append(", ");
						}
					}
				}
				sb.Append(")");
			}
		}

		/// <summary>
		///		Renders the Expression like this:
		///		EACH EXP -> IF EXP NOT ONLY SelfAssignment -> "." END + RenderExpression(IExpression EXP) -> END
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(ExpressionList expression, StringBuilder sb)
		{
			for (var index = 0; index < expression.Expressions.Count; index++)
			{
				var expressionExpression = expression.Expressions[index];
				if (index != 0 && 
				    (expressionExpression as Expression)?.PathParts.All(f => f.Value == PathTokenizer.PathType.SelfAssignment) == false)
				{
					sb.Append(".");
				}
				RenderExpression(expressionExpression, sb);
			}
		}

		public static void RenderExpression(ExpressionString expression, StringBuilder sb)
		{
			sb.Append(expression.Delimiter +
					  string.Join("", expression.StringParts.Select(f => f.PartText))
					  + expression.Delimiter);
		}
	}
}
